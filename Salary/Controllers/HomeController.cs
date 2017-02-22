using IdentitySample.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wages.Calculation;



namespace IdentitySample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Salary", "Home");

            return View();
        }

        [Authorize]
        public ActionResult Salary()
        {
            string userId = User.Identity.GetUserId();
            var dbUsers = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var users = dbUsers.Users.Where(x => x.Id == userId).FirstOrDefault();
            var netSalary = users.EmployeeNettoWage;

            Dictionary<string, string> salaryInfo = new NetToGross().Calculator(netSalary);

            ViewBag.EmployeeName = users.EmployeeName;
            ViewBag.EmployeeSurname = users.EmployeeSurname;
            ViewBag.Email = users.Email;
            ViewBag.salaryInfo = salaryInfo;
            return View();
        }

        //Method to convert bytes array to image
        [Authorize]
        public FileContentResult EmployeePhotos()
        {
            String userId = User.Identity.GetUserId();
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
            // to get the user details to load user Image

            return new FileContentResult(userImage.EmployeePhoto, "image/png");

        }

        //Method for check is bytes array is valid image
        public bool IsPhotoValid(byte[] bytes)
        {
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
    }



}
