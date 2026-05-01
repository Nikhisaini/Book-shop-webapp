using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        
        private readonly IUnitOfWork _unitofwork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType);
            coverType = _unitofwork.coverType.Get(id.GetValueOrDefault());
            if (coverType == null) return BadRequest();
            return View(coverType);
        }
        [HttpPost]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) BadRequest();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _unitofwork.coverType.Add(coverType);
            else
                _unitofwork.coverType.Update(coverType);
            _unitofwork.save();
            return RedirectToAction(nameof(Index));

        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var covertypelist = _unitofwork.coverType.GetAll();
            return Json(new { data = covertypelist });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var catagoryindb = _unitofwork.coverType.Get(id);
            if (catagoryindb == null)
                return Json(new { success = false, message = "Unable To Delete Data!!!" });
            _unitofwork.coverType.Remove(catagoryindb);
            _unitofwork.save();
            return Json(new { success = true, message = "Data Deleted Successfully!!" });
            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
