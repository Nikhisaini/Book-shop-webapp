using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Models.ViewModels;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Graph;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofwork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
          
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM
            {
                product = new Product(),
                CategoryLIst = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),
                CoverTypeList = _unitofwork.coverType.GetAll().Select(ct => new SelectListItem()
                {
                    Text = ct.Name,
                    Value = ct.Id.ToString()
                })
            };
            if (id == null) return View(productVM);
            productVM.product = _unitofwork.product.Get(id.GetValueOrDefault());
            if (productVM.product == null) return BadRequest();
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;//store image to wwwroot folder
                var files = HttpContext.Request.Form.Files;//store uploaded image from html form in a list
                if (files.Count() > 0)//check user uploaded a imager or not
                {
                    var fileName = Guid.NewGuid().ToString(); //genrate unique name for file 
                    var extension = Path.GetExtension(files[0].FileName);//grab orignal extension(.jpg,.png) of file
                    var upload = Path.Combine(webRootPath,"images","products");//save thie image in path we give
                    if (productVM.product.Id != 0)//Edit case
                    {
                        var imageExists = _unitofwork.product.Get(productVM.product.Id).ImageUrl;//grab old image URl from database
                        productVM.product.ImageUrl = imageExists;//set path in  imageExists
                    }
                    if (productVM.product.ImageUrl != null)//Delete old file
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.product.ImageUrl.Trim('\\'));//grab physical image Path from server 
                        if (System.IO.File.Exists(imagePath))//check file exist
                        {
                            System.IO.File.Delete(imagePath);//delete old file from server
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))//create new file in physical path
                    {
                        files[0].CopyTo(fileStream);//copy data in file
                    }
                    productVM.product.ImageUrl = @"/images/products/" + fileName + extension;//save image URL path in database
                }
                else
                {
                    var imageExists = _unitofwork.product.Get(productVM.product.Id).ImageUrl;
                    productVM.product.ImageUrl = imageExists;
                }
                if (productVM.product.Id == 0)
                    _unitofwork.product.Add(productVM.product);//add data in database
                else
                    _unitofwork.product.Update(productVM.product);//update data from database
                _unitofwork.save();//save data in  database
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM = new ProductVM
                {
                    product = new Product(),
                    CategoryLIst = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitofwork.coverType.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    })
                };
                if (productVM.product.Id != 0)
                {
                    productVM.product = _unitofwork.product.Get(productVM.product.Id);
                    if (productVM.product == null) return NotFound();
                }
                return View(productVM);
            }
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var ProductinDb = _unitofwork.product.Get(id);
            if (ProductinDb == null)
                return Json(new { success = false, message = "Unable To Delete Data!!!" });
            //Image Delete
            var webRootPath = _webHostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, ProductinDb.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitofwork.product.Remove(ProductinDb);
            _unitofwork.save();
            return Json(new { success = true, message = "Data Deleted Successfully!!!" });
        }
        #endregion
    }
}
