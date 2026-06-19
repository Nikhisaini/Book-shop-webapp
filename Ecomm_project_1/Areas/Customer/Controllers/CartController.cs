using Ecomm_project_1.DataAccess.Repository;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Models.ViewModels;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stripe;
using Stripe.Checkout;
using Stripe.V2;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Ecomm_project_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public static bool IsEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITwilioService _twilioService;
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager, ITwilioService twilioService)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _twilioService = twilioService;
        }
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claims == null)
            {
                shoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>(),
                    SharedCarts = new List<SharedCart>() 
                };
                return View(shoppingCartVM);
            }

            var count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);

            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "product"),

                SharedCarts = _unitOfWork.sharedCart.GetAll(
                    sc => sc.ApplicationUserId == claims.Value,
                    includeProperties: "ShoppingCart,ShoppingCart.product,ShoppingCart.applicationUser"),

                OrderHeader = new OrderHeader()
            };

            shoppingCartVM.OrderHeader.OrderTotal = 0;
            shoppingCartVM.OrderHeader.applicationUser = _unitOfWork.applicationUser.FirstOrDefault(au => au.Id == claims.Value);

            foreach (var list in shoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnOuentity(list.Count, list.product.Price, list.product.Price50, list.product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.product.Description.Length > 100)
                {
                    list.product.Description = list.product.Description.Substring(0, 99) + "....";
                }
            }

            if (!IsEmailConfirm)
            {
                ViewBag.EmailMessage = "Email Has been sent Kindly Verify your email!";
                ViewBag.EmailCSS = "text-success";
                IsEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email Must be Confirm Authorize Customer";
                ViewBag.EmailCSS = "text-danger";
            }

            return View(shoppingCartVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(int[] selectedItems)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return RedirectToAction(nameof(Index));

            var user = _unitOfWork.applicationUser.FirstOrDefault(u => u.Id == claim.Value);
            if (user == null)
                ModelState.AddModelError(string.Empty, "Email Empty!!!");
            else if (!user.EmailConfirmed)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                string emailBody = $@"
                <div style='font-family: Arial, sans-serif; text-align: center; padding: 40px;'>
                        <h1 style='font-size: 24px; color: #111;'>Thank you for using Book Shopping app</h1>
                        <p style='font-size: 16px; color: #333;'>Please take a moment to make sure we've got your email address right.</p>
                   <div style='margin-top: 30px;'>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                        style='background-color: #e60023; color: white; padding: 15px 25px; text-decoration: none; border-radius: 30px; font-weight: bold; display: inline-block;'>
                         Confirm your email address
                        </a>
                   </div>
                </div>";                
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);

                

                return RedirectToAction(nameof(Index));
            }


            var userCartItems = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList();
            foreach (var cartItem in userCartItems)
            {
                bool Selected = selectedItems.Contains(cartItem.Id);
                if (cartItem.IsSelected != Selected)
                {
                    cartItem.IsSelected = Selected;
                    _unitOfWork.shoppingCart.Update(cartItem);
                }
            }
            _unitOfWork.save();
            var selectedIdsString = string.Join(",", selectedItems);
            HttpContext.Session.SetString("SelectedCartIds", selectedIdsString);
            return RedirectToAction(nameof(summary));
        }


        [HttpPost]
        public IActionResult ShareCart([FromBody] List<string> userEmails)
        {
            var claimsIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claims == null) return Json(new { success = false, message = "Please login to share." });
            if (userEmails == null || !userEmails.Any()) return Json(new { success = false, message = "No emails provided." });

            var myCartItems = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).ToList();

            if (!myCartItems.Any()) return Json(new { success = false, message = "Your cart is empty." });

            int addedCount = 0;

            foreach (var email in userEmails)
            {
                var invitedUser = _unitOfWork.applicationUser.FirstOrDefault(u => u.Email == email);

                if (invitedUser != null && invitedUser.Id != claims.Value)
                {
                    foreach (var item in myCartItems)
                    {
                        var alreadyShared = _unitOfWork.sharedCart
                            .FirstOrDefault(s => s.ShoppingCartId == item.Id && s.ApplicationUserId == invitedUser.Id);
                        if (alreadyShared == null)
                        {
                            _unitOfWork.sharedCart.Add(new SharedCart
                            {
                                ShoppingCartId = item.Id,
                                ApplicationUserId = invitedUser.Id
                            });
                            addedCount++;
                        }
                    }
                }
            }
            _unitOfWork.save();
            return Json(new { success = true, message = $"Cart shared successfully!" });
        }
        [HttpGet]
        public IActionResult GetSharedUsers()
        {
            var claimsIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return Json(new { success = false });

            var hostCartItemIds = _unitOfWork.shoppingCart
                .GetAll(u => u.ApplicationUserId == claims.Value).Select(c => c.Id).ToList();

            var guestUserIds = _unitOfWork.sharedCart
                .GetAll(s => hostCartItemIds.Contains(s.ShoppingCartId))
                .Select(s => s.ApplicationUserId).Distinct().ToList();

            var sharedEmails = _unitOfWork.applicationUser
                .GetAll(u => guestUserIds.Contains(u.Id)).Select(u => u.Email).ToList();

            return Json(new { success = true, emails = sharedEmails });
        }

        [HttpPost]
        public IActionResult RemoveSharedUser([FromBody] string email)
        {
            var claimsIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return Json(new { success = false, message = "Not logged in" });

            var guestUser = _unitOfWork.applicationUser.FirstOrDefault(u => u.Email == email);
            if (guestUser == null) return Json(new { success = false, message = "User not found" });

            var hostCartItemIds = _unitOfWork.shoppingCart
                .GetAll(u => u.ApplicationUserId == claims.Value).Select(c => c.Id).ToList();

            var sharedRecordsToRemove = _unitOfWork.sharedCart
                .GetAll(s => hostCartItemIds.Contains(s.ShoppingCartId) && s.ApplicationUserId == guestUser.Id).ToList();

            foreach (var record in sharedRecordsToRemove)
            {
                _unitOfWork.sharedCart.Remove(record);
            }

            if (sharedRecordsToRemove.Any()) _unitOfWork.save();

            return Json(new { success = true, message = "User removed from group order." });
        }

        public IActionResult plus(int id)
        {
            var cart = _unitOfWork.shoppingCart.Get(id);
            cart.Count += 1;
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cart = _unitOfWork.shoppingCart.Get(id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult delete(int id)
        {
            var cart = _unitOfWork.shoppingCart.Get(id);
            _unitOfWork.shoppingCart.Remove(cart);
            _unitOfWork.save();
            ///return Json(new { success = true, message = "Product Removed From Cart!!!" });
            return RedirectToAction(nameof(Index));

        }
        public IActionResult summary()
        {
            var claimsIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var sessionIds = HttpContext.Session.GetString("SelectedCartIds");
            List<int> selectedIds = null;
            if (!string.IsNullOrEmpty(sessionIds))
            {
                selectedIds = sessionIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                shoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value
                       && selectedIds.Contains(sc.Id), includeProperties: "product").ToList(),
                    OrderHeader = new OrderHeader()
                };
            }
            else
            {
                shoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value
                                   && sc.IsSelected == true, includeProperties: "product").ToList(),
                    OrderHeader = new OrderHeader()
                };
            }
            if (!shoppingCartVM.ListCart.Any())
            {
                TempData["error"] = "Please select at least one item.";
                return RedirectToAction(nameof(Index));
            }

            shoppingCartVM.OrderHeader.OrderTotal = 0;
            shoppingCartVM.OrderHeader.applicationUser = _unitOfWork.applicationUser.FirstOrDefault(au => au.Id == claims.Value);
            foreach (var list in shoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnOuentity(list.Count, list.product.Price, list.product.Price50, list.product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.product.Description.Length > 100)
                {
                    list.product.Description = list.product.Description.Substring(0, 99) + "....";
                }
            }
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.applicationUser.Name;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.applicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.applicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.applicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.applicationUser.PostalCode;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.applicationUser.PhoneNumber;
            return View(shoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("summary")]
        public IActionResult summarypost(string stripetoken)
        {
            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            //
            var sessionIds = HttpContext.Session.GetString("SelectedCartIds");
            List<int> selectedIds = null;
            if (!string.IsNullOrEmpty(sessionIds))
            {
                selectedIds = sessionIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                shoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value
                               && selectedIds.Contains(sc.Id), includeProperties: "product").ToList();
            }
            else
            {
                shoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value
                               && sc.IsSelected == true, includeProperties: "product").ToList();
            }

            if (!shoppingCartVM.ListCart.Any())
                return RedirectToAction(nameof(Index));

            shoppingCartVM.OrderHeader.applicationUser = _unitOfWork.applicationUser.FirstOrDefault(au => au.Id == claims.Value);

            shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.orderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.save();
            foreach (var list in shoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnOuentity(list.Count, list.product.Price, list.product.Price50, list.product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    ProductId = list.ProductId,
                    Price = list.Price,
                    Count = list.Count
                };
                _unitOfWork.orderDetail.Add(orderDetail);

            }
            _unitOfWork.save();
            //Remove From ShoppingCart
            _unitOfWork.shoppingCart.RemoveRange(shoppingCartVM.ListCart);
            _unitOfWork.save();
            //session set
            var Count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            //stripe payment
            if (stripetoken == null)
            {
                shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                shoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(shoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "order Id:" + shoppingCartVM.OrderHeader.ToString(),
                    Source = stripetoken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    shoppingCartVM.OrderHeader.TransectionId = charge.Id;
                if (charge.Status.ToLower() == "succeeded")
                {
                    shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }
                _unitOfWork.save();
            }
            _unitOfWork.orderHeader.Update(shoppingCartVM.OrderHeader);
            _unitOfWork.save();
            return RedirectToAction("orderconfirmation", "Cart",
                new { id = shoppingCartVM.OrderHeader.Id });
        }
        [HttpPost]
        [ActionName("RazorpayCheckOut")]
        public IActionResult RazorpayCheckOut(string transactionId)
        {
            shoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader(),
                ListCart = new List<ShoppingCart>()
            };
            // ← ADD THESE — read form fields manually
            shoppingCartVM.OrderHeader.Name = Request.Form["orderHeader.Name"];
            shoppingCartVM.OrderHeader.PhoneNumber = Request.Form["orderHeader.PhoneNumber"];
            shoppingCartVM.OrderHeader.StreetAddress = Request.Form["orderHeader.StreetAddress"];
            shoppingCartVM.OrderHeader.City = Request.Form["orderHeader.City"];
            shoppingCartVM.OrderHeader.State = Request.Form["orderHeader.State"];
            shoppingCartVM.OrderHeader.PostalCode = Request.Form["orderHeader.PostalCode"];

            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            //
            var sessionIds = HttpContext.Session.GetString("SelectedCartIds");
            List<int> selectedIds = null;
            if (!string.IsNullOrEmpty(sessionIds))
            {
                selectedIds = sessionIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                shoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value
                               && selectedIds.Contains(sc.Id), includeProperties: "product").ToList();
            }
            else
            {
                shoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value
                               && sc.IsSelected == true, includeProperties: "product").ToList();
            }

            if (!shoppingCartVM.ListCart.Any())
                return RedirectToAction(nameof(Index));

            shoppingCartVM.OrderHeader.applicationUser = _unitOfWork.applicationUser.FirstOrDefault(au => au.Id == claims.Value);

            shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.orderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.save();
            foreach (var list in shoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnOuentity(list.Count, list.product.Price, list.product.Price50, list.product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    ProductId = list.ProductId,
                    Price = list.Price,
                    Count = list.Count
                };
                _unitOfWork.orderDetail.Add(orderDetail);

            }
            // 10% discount then 5% GST on discounted amount
            double discount = Math.Round(shoppingCartVM.OrderHeader.OrderTotal * 0.10, 2);
            double discountedTotal = shoppingCartVM.OrderHeader.OrderTotal - discount;
            double gst = Math.Round(discountedTotal * 0.05, 2);
            shoppingCartVM.OrderHeader.OrderTotal = Math.Round(discountedTotal + gst, 2);

            _unitOfWork.save();
            //Remove From ShoppingCart
            _unitOfWork.shoppingCart.RemoveRange(shoppingCartVM.ListCart);
            _unitOfWork.save();
            //session set
            var Count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            // ── Razorpay payment only ────────────────────────────────
            if (string.IsNullOrEmpty(transactionId) || transactionId == "rejected")
            {
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                TempData["error"] = "Payment Failed! Please try again.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                shoppingCartVM.OrderHeader.TrackingNumber = transactionId;
                shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            }
            _unitOfWork.orderHeader.Update(shoppingCartVM.OrderHeader);
            _unitOfWork.save();
            return RedirectToAction(nameof(orderconfirmation),
                new { id = shoppingCartVM.OrderHeader.Id });

        }

        public async Task<IActionResult> orderconfirmation(int id)
        {
            var orderHeader = _unitOfWork.orderHeader.FirstOrDefault(u => u.Id == id, includeProperties: "applicationUser");
            var orderDetails = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "product");

            var productNames = orderDetails.Select(u => u.product.Title).ToList();
            var customerName = orderHeader.Name ?? "Customer";

            if (orderHeader != null)
            {
                string productTableRows = "";
                foreach (var item in orderDetails)
                {
                    string imagePath = "https://yourwebsite.com" + item.product.ImageUrl;
                    productTableRows += $@"
                    <tr>                  
                      <td style='padding:10px; border-bottom:1px solid #eee;'>
                      <img src='{imagePath}' width='50' height='50' style='border-radius:5px;' />
                      </td>
                      <td style='padding:10px; border-bottom:1px solid #eee;'>{item.product.Title}</td>
                      <td style='padding:10px; border-bottom:1px solid #eee; text-align:center;'>{item.Count}</td>
                      <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>₹{item.Price}</td>
                      <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>₹{item.Price * item.Count}</td>
                    </tr>";
                }
                string emailHtml = $@"
                    <div style='font-family: sans-serif; max-width: 600px; border: 1px solid #ddd; padding: 20px;'>
                      <h2 style='color: #2c3e50;'>Order Confirmation</h2>
                      <p>Hi {orderHeader.Name}, your order <b>#{id}</b> is confirmed!</p>     
                <table style='width:100%; border-collapse: collapse;'>
                    <thead>
                     <tr style='background: #f8f9fa;'>
                        <th style='text-align:left; padding:10px;'>Image</th>
                        <th style='text-align:left; padding:10px;'>Product</th>
                        <th style='padding:10px;'>Qty</th>
                        <th style='text-align:right; padding:10px;'>Price</th>
                        <th style='text-align:right; padding:10px;'>Total</th>
                     </tr>
                   </thead>
                 <tbody>
                      {productTableRows}
                 </tbody>
               </table>
                   <div style='text-align:right; margin-top:20px; font-size: 1.2em;'>
                     <b>Grand Total: ₹{orderHeader.OrderTotal}</b>
                   </div>
                     <p style='margin-top:30px; font-size:0.9em; color:#777;'>Thank you for shopping with us!</p>
                   </div>";
                try
                {
                    await _twilioService.SendOrderConfirmationSmsAsync(orderHeader.PhoneNumber, id, customerName, productNames);
                    await _twilioService.MakeOrderConfirmationCallAsync(orderHeader.PhoneNumber, id, customerName, productNames);
                    await _emailSender.SendEmailAsync(orderHeader.applicationUser.Email, $"Order Confirmed #{id}", emailHtml);
                    await _twilioService.SendOrderConfirmationWhatsAppAsync(orderHeader.PhoneNumber, id, customerName, productNames);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Twilio Notification Failed: {ex.Message}");
                }
            }
            return View(id);
        }
    }
}