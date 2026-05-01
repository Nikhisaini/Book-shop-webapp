using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);
            company = _unitOfWork.company.Get(id.GetValueOrDefault());
            if (company == null) return BadRequest();
            return View(company);
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return BadRequest();
            if (!ModelState.IsValid) return View(company);
            if (company.id == 0)
                _unitOfWork.company.Add(company);
            else
                _unitOfWork.company.Update(company);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.company.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyindb = _unitOfWork.company.Get(id);
            if (companyindb == null)
                return Json(new { success = false, message = "Unable To Delete Data!!!" });
            _unitOfWork.company.Remove(companyindb);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data Deleted Successfully!!" });
        }
        #endregion
    }
}
