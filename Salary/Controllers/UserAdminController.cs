using IdentitySample.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using Wages.Calculation;

namespace IdentitySample.Controllers
{
    /*public class CustomAuthorizeAttribute : AuthorizeAttribute
    {

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
                return;
            }
        }
    }*/


    [Authorize(Roles = "Admin, Manager")]
    public class UsersAdminController : Controller
    {
        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Users/
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }

        // Method returns information about employee
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {

            string userId = id;
            var dbUsers = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var users = dbUsers.Users.Where(x => x.Id == userId).FirstOrDefault();
            var netSalary = users.EmployeeNettoWage;

            Dictionary<string, string> salaryInfo = new NetToGross().Calculator(netSalary);

            ViewBag.salaryInfo = salaryInfo;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        //Method to convert bytes array to image
        [Authorize]
        public FileContentResult EmployeePhotos(string id)
        {
            //Selecting employee's photo by id
            String userId = id;
            var bdUsers = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var userImage = bdUsers.Users.Where(x => x.Id == userId).FirstOrDefault();

            if (userId == null || userImage.EmployeePhoto.Length == 0 || !IsPhotoValid(userImage.EmployeePhoto))
            {
                string fileName = HttpContext.Server.MapPath(@"~/Content/noimg.png");

                byte[] imageData = null;
                FileInfo fileInfo = new FileInfo(fileName);
                long imageFileLength = fileInfo.Length;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                imageData = br.ReadBytes((int)imageFileLength);

                return File(imageData, "image/png");

            }
            // To get the user details to load user Image

            return new FileContentResult(userImage.EmployeePhoto, "image/jpg");
        }

        // Method to check is bytes array is valid image
        public bool IsPhotoValid(byte[] bytes)
        {
            if (bytes == null)
                return false;

            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                    Image.FromStream(ms);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        // Methods return information about employee to edit form
        // GET: /Users/Edit/1
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {

            // Return info about salary by employee's id as ViewBag.salaryInfo
            string userId = id;
            var dbUsers = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var users = dbUsers.Users.Where(x => x.Id == userId).FirstOrDefault();
            var netSalary = users.EmployeeNettoWage;

            Dictionary<string, string> salaryInfo = new NetToGross().Calculator(netSalary);

            ViewBag.salaryInfo = salaryInfo;


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,

                // Custom employee information for Name, Surname and NettoWage
                EmployeeName = user.EmployeeName,
                EmployeeSurname = user.EmployeeSurname,
                EmployeeNettoWage = user.EmployeeNettoWage,
                EmployeePhoto = user.EmployeePhoto,

                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        // If everything is fine method will save updated infor about employee
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Exclude = "EmployeePhoto", Include = "Email,Id,EmployeeName,EmployeeSurname,EmployeeNettoWage")]  EditUserViewModel editUser, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {

                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // To convert the employee's uploaded photo as byte array before put in database
                byte[] imageData = null;
                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase poImgFile = Request.Files["EmployeePhoto"];

                    using (var binary = new BinaryReader(poImgFile.InputStream))
                    {
                        imageData = binary.ReadBytes(poImgFile.ContentLength);
                    }
                }

                var labas = User.Identity.Name;



                user.UserName = editUser.Email;
                user.Email = editUser.Email;
                user.EmployeeName = editUser.EmployeeName;
                user.EmployeeSurname = editUser.EmployeeSurname;
                user.EmployeeNettoWage = editUser.EmployeeNettoWage;

                // If photo is not uploaded program won't change it
                if(imageData.Length != 0)
                    user.EmployeePhoto = imageData;

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }

                return RedirectToAction("Edit", new { id = user.Id });
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        // Method return employee's id to confirm choice
        // GET: /Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // Method delete employee if confirmed
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
