using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Models.ViewModels;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace Ecomm_project_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult PreviousOrders()
        {
            return View();
        }
        public IActionResult Details(int orderId, int productId)
        {
            OrderDetailVM orderVM = new OrderDetailVM()
            {
                OrderHeader = _unitOfWork.orderHeader.FirstOrDefault(u => u.Id == orderId, includeProperties: "applicationUser"),
                OrderDetails = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == orderId && u.ProductId == productId, 
                includeProperties: "product")
            };
            return View(orderVM);
        }
        [HttpPost]
        public IActionResult ReorderItem(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var cartFromDb = _unitOfWork.shoppingCart.FirstOrDefault(
                u => u.ApplicationUserId == userId && u.ProductId == productId
            );
            if (cartFromDb != null)
            {
                cartFromDb.Count += 1;
            }
            else
            {
                ShoppingCart cart = new ShoppingCart()
                {
                    ProductId = productId,
                    ApplicationUserId = userId,
                    Count = 1
                };
                _unitOfWork.shoppingCart.Add(cart);
            }
            _unitOfWork.save();
            return RedirectToAction("summary", "Cart");
        }
        #region API 
        [HttpGet]
        public IActionResult GetAllPreviousOrders()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var userOrderDetails = _unitOfWork.orderDetail.GetAll(u => u.orderHeader.ApplicationUserId == userId,
                includeProperties: "orderHeader,product");

            var distinctProductOrders = userOrderDetails.OrderByDescending(o => o.orderHeader.Id).GroupBy(o => o.ProductId)
                .Select(g => g.First()).Select(o => new
                {
                    orderId = o.OrderHeaderId,
                    productId = o.ProductId,
                    name = o.orderHeader.Name,
                    state = o.orderHeader.State,
                    productName = o.product.Title
                }).ToList();

            return Json(new { data = distinctProductOrders });
        }
        #endregion
    }
}