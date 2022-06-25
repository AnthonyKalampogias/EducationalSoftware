using EducationalSoftware.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalSoftware.Controllers
{
    public class ContentController : Controller
    {
        public ActionResult Index(int? contentID = null)
        {
            if(contentID == null)
            {
                ViewData["error"] = "You need to select a chapter to read first!";
                return RedirectToAction("Index", "Home");
            }

            var content = new Content();
            using (var db = new SoftwareEduEntities())
                content = db.Content.FirstOrDefault(x => x.Id == contentID);
            return View(content);
        }


        public ActionResult Quiz(int? contentID = null)
        {
            if (contentID == null)
            {
                ViewData["error"] = "You need to select a chapter to start a quiz!";
                return RedirectToAction("Index", "Home");
            }
            var questionList = new List<Tests>();
            using (var db = new SoftwareEduEntities())
                questionList = db.Tests.Where(x => x.Chapter == contentID).Include(x => x.Content).ToList();
            return View(questionList);
        }

        [HttpPost]
        public ActionResult submitAnswers(string values)
        {
            return View("Quiz");
        }
    }
}