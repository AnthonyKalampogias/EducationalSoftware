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
                
                if(content == null)
                    //there is a chance that the ID doesnt match so get the list of content for that chapter and fetch the first
                    content = db.Content.First(x => x.chapId == contentID);

                var contentList = db.Content.Where(x => x.chapId == content.chapId).ToList();
                var nextId = contentList.IndexOf(content) + 1 < contentList.Count ? contentList[contentList.IndexOf(content) + 1].Id : 0;
                ViewData["NextChapter"] = db.Content.FirstOrDefault(x => x.Id == nextId);
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
                questionList = db.Tests.Where(x => x.ContentId == contentID).Include(x => x.Content).ToList();

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
            var suggestions = new List<int>();
            for (int i = 0; i < correctAnswers.Count; i++)
            {
                if (correctAnswers[i].answerA == answers[i])
                    score++;
                else
                    suggestions.Add(correctAnswers[i].ContentId);
            }

            using (var db = new SoftwareEduEntities())
            {
                db.Scores.Add(new Scores
                {
                    Score = score,
                    ContentId = chapter,
                    Total = correctAnswers.Count,
                    UserID = (int)Session["id"],
                    testDate = DateTime.Now,
                    Suggestion = ((double)score / (double)correctAnswers.Count) * 100 < 75 ? JsonConvert.SerializeObject(suggestions) : null
                });
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { contentID = chapter });
        }
        #endregion

        #region Scores methods
        public ActionResult MyScores(int? contentID = null, DateTime? selectedDate = null)
        {
            TempData["contentId"] = contentID;
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
                        .Where(x => x.ContentId == contentID && x.UserID == userId)
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