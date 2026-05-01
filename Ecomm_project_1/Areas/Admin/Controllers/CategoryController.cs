using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Catagory catagory = new Catagory();
            if (id == null) return View(catagory);
            catagory = _unitofwork.category.Get(id.GetValueOrDefault());
            if (catagory == null) return NotFound();
            return View(catagory);
        }
        [HttpPost]

        public IActionResult Upsert(Catagory catagory)
        {
            if (catagory == null) return BadRequest();
            if (!ModelState.IsValid) return View(catagory);
            if (catagory.Id == 0)
                _unitofwork.category.Add(catagory);
            else
                _unitofwork.category.Update(catagory);
            _unitofwork.save();
            return RedirectToAction(nameof(Index));
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var CategoryList = _unitofwork.category.GetAll();
            return Json(new { data = CategoryList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var categoryindb = _unitofwork.category.Get(id);
            if (categoryindb == null)
                return Json(new { success = false, message = "Unable To Delete Data!!!!" });
            _unitofwork.category.Remove(categoryindb);
            _unitofwork.save();
            return Json(new { success = true, message = "Data Deleted Successfully!!!" });
            return RedirectToAction(nameof(Index));
            
        }
        #endregion
    }
}

