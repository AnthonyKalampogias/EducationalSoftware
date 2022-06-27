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

            //TODO BUG get quiz if nothing on ID get error
            //TODO ! add greek ....
            //TODO ! fix suggestions code

            //TODO OPTION add buttons to do stuff from content
            
            return View();
        }
    }
}