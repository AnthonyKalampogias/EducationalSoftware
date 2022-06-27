using System;
using System.Collections.Generic;
using EducationalSoftware.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalSoftware.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if(Session["id"] != null)
            {
                using (var db = new SoftwareEduEntities())
                    ViewData["Chapters"] = db.Chapters.ToList();
            }

            //TODO get quiz if nothing on ID get error
            //TODO find a way to save final tests in DB, can't add null to contentID
            //TODO add greek ....
            //TODO fix suggestions code
            
            return View();
        }
    }
}