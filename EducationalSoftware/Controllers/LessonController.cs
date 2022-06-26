using EducationalSoftware.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalSoftware.Controllers
{
    public class LessonController : Controller
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
            {
                content = db.Content.FirstOrDefault(x => x.Id == contentID);
                ViewData["NextChapter"] = db.Content.FirstOrDefault(x => x.Id == contentID + 1);
            }
                
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

            TempData["correctAnswers"] = questionList;
            TempData["contentID"] = contentID;
            return View(questionList);
        }

        [HttpPost]
        public ActionResult submitAnswers(string values)
        {
            var chapter = (int)TempData["contentID"];
            var answers = JsonConvert.DeserializeObject<List<string>>(values);
            var correctAnswers = (List<Tests>)TempData["correctAnswers"];

            int score = 0;
            var suggestions = new List<string>();
            for (int i = 0; i < correctAnswers.Count; i++)
            {
                if (correctAnswers[i].answerA == answers[i])
                    score++;
                else
                    suggestions.Add(correctAnswers[i].Content.Title.Split(' ').Last()+ " " + correctAnswers[i].Question.Replace("\n", ""));
            }

            using (var db = new SoftwareEduEntities())
            {
                db.Scores.Add(new Scores
                {
                    Score = score,
                    ChapId = chapter,
                    Total = correctAnswers.Count,
                    UserID = (int)Session["id"],
                    Suggestion = JsonConvert.SerializeObject(suggestions)
                });
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { contentID = chapter });
        }
    }
}