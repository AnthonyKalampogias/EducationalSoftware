using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalSoftware.Controllers
{
    public class ContentController : Controller
    {
        public ActionResult Index(int? contentID = null)
        {

            if(contentID != null)
            {

            }

            return View();
        }
    }
}