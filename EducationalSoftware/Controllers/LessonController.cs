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
        #region Show Chapter
        public ActionResult Index(int? contentID = null)
        {
            if (contentID == null)
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
        #endregion

        #region Quiz Methods
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
                    suggestions.Add(correctAnswers[i].Content.Title.Split(' ').Last() + " " + correctAnswers[i].Question.Replace("\n", "").Replace("\r", ""));
            }

            using (var db = new SoftwareEduEntities())
            {
                db.Scores.Add(new Scores
                {
                    Score = score,
                    ChapId = chapter,
                    Total = correctAnswers.Count,
                    UserID = (int)Session["id"],
                    testDate = DateTime.Now,
                    Suggestion = JsonConvert.SerializeObject(suggestions)
                });
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { contentID = chapter });
        }
        #endregion

        #region Scores methods
        public ActionResult MyScores(int? contentID = null, DateTime? selectedDate = null)
        {
            if (Session["id"] == null)
            {
                ViewData["error"] = "You need to select a chapter first to see your scores!";
                return RedirectToAction("Index", "Home");
            }

            var userId = (int)Session["id"];
            var myScores = new List<Scores>();

            using (var db = new SoftwareEduEntities())
            {
                if (contentID == null)
                    myScores = db.Scores
                        .Where(x => x.UserID == userId)
                        .Include(x => x.Content)
                        .ToList(); // get scores for all chapters
                else
                    myScores = db.Scores
                        .Where(x => x.ChapId == contentID && x.UserID == userId)
                        .Include(x => x.Content)
                        .ToList(); // get for specific chapter
            }

            if (selectedDate != null)
            {
                ViewData["datePick"] = selectedDate;
                myScores = myScores.Where(x => x.testDate.Date == selectedDate.Value.Date).ToList();
            }
            return View(myScores);
        }
        #endregion
    }
}