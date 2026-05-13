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
            if(claims != null)
            {
                var count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }

            //**
            IEnumerable<Product> productlist = _unitOfWork.product.GetAll(includeProperties: "catagory");//show all product list \
            // 2. NEW: Count how many books are in each category automatically!
            var categoryCounts = productlist.Where(p => p.catagory != null).GroupBy(p => p.catagory.Name).ToDictionary(g => g.Key, g => g.Count());

            // Send the dictionary of counts to the HTML page
            ViewBag.CategoryCounts = categoryCounts;
            if (!string.IsNullOrEmpty(category))
            {
               
                productlist = productlist.Where(p => p.catagory.Name == category).ToList();
            }
            return View(productlist); //return productlist to view
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
                shoppingCart.ApplicationUserId = claims.Value;
                var shoppingcartinDb = _unitOfWork.shoppingCart.FirstOrDefault(sc => sc.ApplicationUserId ==
                claims.Value && sc.ProductId == shoppingCart.ProductId);

                if (shoppingcartinDb == null)
                {
                    shoppingCart.IsSelected = true;
                    _unitOfWork.shoppingCart.Add(shoppingCart);
                }
                else
                    shoppingcartinDb.Count += shoppingCart.Count;
                 _unitOfWork.save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
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
