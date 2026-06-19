using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Models.ViewModels;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AllOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AllOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }       
        public IActionResult DateWiseSearch()
        {
            return View();
        }
        public IActionResult MonthWiseOrder()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var orderVM = new OrderDetailVM
            {
                OrderHeader = _unitOfWork.orderHeader.FirstOrDefault(u => u.Id == id, includeProperties: "applicationUser"),
                OrderDetails = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "product")
            };
            return View(orderVM);
        }
        public IActionResult Cancel(int id)
        {
            var orderHeader = _unitOfWork.orderHeader.FirstOrDefault(u => u.Id == id);
            if (orderHeader == null)
            {
                return NotFound();
            }

            // --- STRIPE REFUND LOGIC ---
            // Only attempt a refund if the payment was actually approved
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransectionId // Make sure this matches your DB column name
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                // Update payment status to refunded
                orderHeader.OrderStatus = SD.OrderStatusRefunded;
                orderHeader.PaymentStatus = SD.PaymentStatusRefunded;
            }

            // Update order status to cancelled for all cancelled orders
            orderHeader.OrderStatus = SD.OrderStatusCancelled;

            _unitOfWork.orderHeader.Update(orderHeader);
            _unitOfWork.save();

            return RedirectToAction("Details", new { id = id });
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll(string status = "All")
        {
            var orderList = _unitOfWork.orderHeader.GetAll(includeProperties: "applicationUser");

            return Json(new { data = orderList });
        }

        [HttpGet]
        public IActionResult GetSearchDate(DateTime? start = null, DateTime? end = null, string status = "All")
        {
            if (start == null && end == null && status == "All")
            {
                return Json(new { data = new List<OrderHeader>() });
            }

            IEnumerable<OrderHeader> orderList = _unitOfWork.orderHeader.GetAll();
            if (start != null && end != null)
            {
                orderList = orderList.Where(u => u.OrderDate >= start && u.OrderDate <= end);
            }
            if (status != "All")
            {
                orderList = orderList.Where(u => u.OrderStatus == status);
            }
            return Json(new { data = orderList });
        }
        [HttpGet]
        public IActionResult GetByMonth(int month)
        {
            IEnumerable<OrderHeader> orderHeaderList;

            if (month == 0) 
            {
                orderHeaderList = _unitOfWork.orderHeader.GetAll(includeProperties: "applicationUser");
            }
            else
            {
                orderHeaderList = _unitOfWork.orderHeader.GetAll(u => u.OrderDate.Month == month,includeProperties: "applicationUser");
            }

            return Json(new { data = orderHeaderList });
        }

        #endregion
    }
}
