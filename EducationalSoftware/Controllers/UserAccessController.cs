using EducationalSoftware.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalSoftware.Controllers
{
    public class UserAccessController : Controller
    {
        // GET: UserAccess
        public ActionResult Login()
        {
            if (TempData["message"] != null)
                ViewBag.message = TempData["message"];
            return View();
        }

        public ActionResult LoginUser(Users user)
        {
            if (user == null)
                return RedirectToAction("Login");

            using (var db = new SoftwareEduEntities())
            {
                var foundUser =
                    db.Users.FirstOrDefault(usr =>
                        usr.Email == user.Email &&
                        usr.Password == user.Password
                    );

                if (foundUser == null)
                    return RedirectToAction("Login");

                Session["id"] = foundUser.Id;
                Session["name"] = foundUser.FirstName + " " + foundUser.LastName;
                Session["type"] = foundUser.Status;
            }

            return RedirectToAction("Index", "Home");
         }

        public ActionResult Register()
        {
            if (TempData["message"] != null)
                ViewBag.message = TempData["message"];
            return View();
        }

        public ActionResult UserRegister(Users usr)
        {
            using(var db = new SoftwareEduEntities())
            {
                var alreadyRegistered = db.Users.Any(x => x.Email == usr.Email);

                if (alreadyRegistered)
                {
                    ViewBag.message = "Already registered email"; 
                    return RedirectToAction("Register");
                }

                usr.Status = 0;
                db.Users.Add(usr);
                db.SaveChanges();
            }
            return RedirectToAction("LoginUser", "UserAccess", new { user = usr });
        }

        public ActionResult Logout()
        {
            try
            {
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return RedirectToAction("Index", "Home");
            }
        }

    }
}