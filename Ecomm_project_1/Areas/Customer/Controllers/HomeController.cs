using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Models.ViewModels;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Ecomm_project_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(string category = null)
        {
            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            IEnumerable<Product> productlist = _unitOfWork.product.GetAll(includeProperties: "catagory");
            var categoryCounts = productlist.Where(p => p.catagory != null).GroupBy(p => p.catagory.Name)
                            .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.CategoryCounts = categoryCounts;

            var bestSellers = productlist.Where(p => p.IsBestseller == true).OrderByDescending(p => p.SalesCount).Take(12).ToList();

            ViewBag.BestSellers = bestSellers;

            var newArrivals = productlist.OrderByDescending(p => p.Id).Take(8).ToList();
            ViewBag.NewArrivals = newArrivals;

            if (!string.IsNullOrEmpty(category))
            {
                productlist = productlist.Where(p => p.catagory != null && p.catagory.Name == category).ToList();
            }
            return View(productlist);
        }
        public IActionResult Details(int id)
        {
            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            //*****
            var productInDb = _unitOfWork.product.FirstOrDefault(p => p.Id == id, includeProperties: "catagory,coverType"); 
            if (productInDb == null) return BadRequest();
            var shoppingCart = new ShoppingCart()
            {
               product= productInDb,
                ProductId = id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;

            if (ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)(User.Identity);
                var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claims == null) return NotFound();

                var currentUserId = claims.Value;

                // ==========================================
                // 1. GROUP ORDER INTERCEPTION LOGIC
                // ==========================================
                // Check if this user is a guest in an active group order
                var activeGroupOrder = _unitOfWork.sharedCart
                    .FirstOrDefault(s => s.ApplicationUserId == currentUserId, includeProperties: "ShoppingCart");

                string targetCartOwnerId = currentUserId; // Default to their own cart

                if (activeGroupOrder != null)
                {
                    // User is a Guest! Re-route the item to the Host's cart
                    targetCartOwnerId = activeGroupOrder.ShoppingCart.ApplicationUserId;
                }

                // Assign the item to the correct owner (Host or Self)
                shoppingCart.ApplicationUserId = targetCartOwnerId;

                // Check if item exists in the TARGET cart
                var shoppingcartinDb = _unitOfWork.shoppingCart.FirstOrDefault(sc =>
                    sc.ApplicationUserId == targetCartOwnerId &&
                    sc.ProductId == shoppingCart.ProductId);

                if (shoppingcartinDb == null)
                {
                    // Kept your specific logic here!
                    shoppingCart.IsSelected = true;

                    _unitOfWork.shoppingCart.Add(shoppingCart);
                    _unitOfWork.save(); // Save here to generate the new shoppingCart.Id

                    // ==========================================
                    // 2. SHARE-BACK LOGIC FOR GUESTS
                    // ==========================================
                    // If a guest added this to the host's cart, create a SharedCart record
                    // so the guest can see the item they just added on their own screen.
                    if (currentUserId != targetCartOwnerId)
                    {
                        SharedCart newSharedLink = new SharedCart
                        {
                            ShoppingCartId = shoppingCart.Id, // The new ID we just generated
                            ApplicationUserId = currentUserId // The Guest's ID
                        };
                        _unitOfWork.sharedCart.Add(newSharedLink);
                        _unitOfWork.save();
                    }
                }
                else
                {
                    // Item already exists, just increase count
                    shoppingcartinDb.Count += shoppingCart.Count;
                    _unitOfWork.save();
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Kept your fallback logic exactly the same!
                var productInDb = _unitOfWork.product.FirstOrDefault(p => p.Id == shoppingCart.Id,
                    includeProperties: "catagory,coverType");

                if (productInDb == null) return BadRequest();

                var shoppingCartEdit = new ShoppingCart()
                {
                    product = productInDb,
                    ProductId = shoppingCart.Id
                };

                return View(shoppingCartEdit);
            }
        }
        public IActionResult EmptyCart()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
