using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models.ViewModels;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ReportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult AllTopUsers()
        {
            // Fetch all approved orders including the Identity User details
            var orders = _unitOfWork.orderHeader.GetAll(
                u => u.PaymentStatus == "Approved",
                includeProperties: "applicationUser"
            );

            // Perform aggregation in memory
            var topUsers = orders
                .GroupBy(o => new {
                    o.applicationUser.Name,
                    o.applicationUser.Email,
                    o.applicationUser.PhoneNumber
                })
                .Select(g => new TopUserVM
                {
                    Name = g.Key.Name,
                    Email = g.Key.Email,
                    PhoneNumber = g.Key.PhoneNumber,
                    TotalSpent = g.Sum(o => o.OrderTotal)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToList();

            return View(topUsers);
        }
        public IActionResult MonthlyTopUsers(int? month, int? year)
        {
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            // Fetch approved orders filtered by the selected month and year
            var orders = _unitOfWork.orderHeader.GetAll(
                o => o.PaymentStatus == "Approved" &&
                     o.OrderDate.Month == selectedMonth &&
                     o.OrderDate.Year == selectedYear,
                includeProperties: "applicationUser"
            );

            var monthlyTopUsers = orders
                .GroupBy(o => new {
                    o.applicationUser.Name,
                    o.applicationUser.Email,
                    o.applicationUser.PhoneNumber
                })
                .Select(g => new TopUserVM
                {
                    Name = g.Key.Name,
                    Email = g.Key.Email,
                    PhoneNumber = g.Key.PhoneNumber,
                    TotalSpent = g.Sum(o => o.OrderTotal)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToList();

            return View(monthlyTopUsers);
        }
    }
}
