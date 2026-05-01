using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _context.applicationUsers.ToList();//aspnetusers
            var roleList = _context.Roles.ToList();//aspnetrole
            var userRole = _context.UserRoles.ToList();//aspnetuserrole
            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roleList.FirstOrDefault(r => r.Id == roleId).Name;
                if (user.CompanyID == null)
                {
                    user.company = new Company()
                    {
                        Name = ""
                    };
                }
                if (user.CompanyID != null)
                {
                    user.company = new Company()
                    {
                        Name = _unitOfWork.company.Get(Convert.ToInt32(user.CompanyID)).Name
                    };
                }
            }
            //Remove Admin Role User
            var adminUser = userList.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userList.Remove(adminUser);

            return Json(new { data = userList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            bool isLocked = false;
            var userinDb = _unitOfWork.applicationUser.FirstOrDefault(u => u.Id == id);
            if (userinDb == null)
            {
                return Json(new { success = false, message = "Something Went Wrong While Lock And Unlock User!!!" });
            }
            if (userinDb != null && userinDb.LockoutEnd > DateTime.Now)
            {
                userinDb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userinDb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = isLocked == true ? "User Successfully Locked" : "User Successfully Unlockd" });
        }
        #endregion
    }
}
