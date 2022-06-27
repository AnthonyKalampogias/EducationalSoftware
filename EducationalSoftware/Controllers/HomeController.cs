﻿using System;
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

            //TODO ! add greek ....
            //TODO ! docs 🙄

            //TODO OPTION add buttons to do stuff from content
            
            return View();
        }
    }
}