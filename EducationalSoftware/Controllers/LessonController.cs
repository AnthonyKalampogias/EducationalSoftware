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
                TempData["error"] = "You need to select a chapter to read first!";
                return RedirectToAction("Index", "Home");
            }

            var content = new Content();
            using (var db = new SoftwareEduEntities())
            {
                content = db.Content.FirstOrDefault(x => x.Id == contentID); 
                
                if(content == null)
                    //there is a chance that the ID doesnt match so get the list of content for that chapter and fetch the first
                    if(db.Content.Any(x => x.chapId == contentID))
                        content = db.Content.First(x => x.chapId == contentID);
                    else
                    {
                        TempData["error"] = "Something went wrong, pleasae try again!";
                        return RedirectToAction("Index", "Home");
                    }
                var contentList = db.Content.Where(x => x.chapId == content.chapId).ToList();
                var nextId = contentList.IndexOf(content) + 1 < contentList.Count ? contentList[contentList.IndexOf(content) + 1].Id : 0;
                ViewData["NextChapter"] = db.Content.FirstOrDefault(x => x.Id == nextId);
            }
            
            if (userCanDoFinal(content.chapId))
                ViewData["submitFinalTest"] = true;

            return View(content);
        }
        #endregion

        #region Quiz Methods
        public ActionResult Quiz(int? contentID = null)
        {
            if (contentID == null)
            {
                TempData["error"] = "You need to select a chapter to start a quiz!";
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
        public void submitAnswers(string values)
        {
            var content = (int)TempData["contentID"];
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
                    ContentId = content,
                    Total = correctAnswers.Count,
                    UserID = (int)Session["id"],
                    testDate = DateTime.Now,
                    Suggestion = ((double)score / (double)correctAnswers.Count) * 100 < 75 ? JsonConvert.SerializeObject(suggestions) : null
                });
                db.SaveChanges();
            }
            TempData["success"] = $"You have succesfully completed the test for {correctAnswers.First().Content.Title}, you can navigate to My Results to see how you did!";
        }

        #endregion

        #region Final Quiz Methods
        public ActionResult Final(int chaterpId)
        {
            #region Initials
            if (chaterpId == 0)
            {
                TempData["error"] = "You need to select a chapter to start the final quiz!";
                return RedirectToAction("Index", "Home");
            }

            if (!userCanDoFinal(chaterpId))
            {
                TempData["error"] = "You haven't completed all the chapter quized to initiate the final quiz!";
                return RedirectToAction("Index", "Home");
            }

            TempData["chapterID"] = chaterpId;
            var usrId = (int)Session["id"];
            var finalQuestionsAmount = 25;
            #endregion

            #region Get data from DB

            //Bring all chapters content with their questions
            /// and previous scores if available
            var content = new List<Content>();
            var contentIds = new List<int>();
            var previousScores = new List<Scores>();

            using (var db = new SoftwareEduEntities())
            {
                ViewData["chapName"] = db.Chapters.FirstOrDefault(x => x.Id == chaterpId).ChapterTitle;
                content = db.Content.Where(x => x.chapId == chaterpId).Include(x => x.Tests).ToList();
                contentIds = content.Select(p => p.Id).ToList();
                // get previous scores for this chapters contents, get content ids and see if the score id is in those Ids
                previousScores = db.Scores.Where(x => x.UserID == usrId && contentIds.Contains(x.ContentId)).ToList();
            }
            #endregion

            #region Create and fill the Questions List

            // make a fair split of questions between all content questions based on the number of them
            var takeFromEachContent = finalQuestionsAmount / contentIds.Count();

            // list to add content IDs that have been add for the final test
            var checkedIds = new List<int>();

            // fill X amount of questions for the final test
            var finalsQ = new List<Tests>();
            while (finalsQ.Count < finalQuestionsAmount)
            {
                //First check in previous tests if the student needs to improve in something
                if (previousScores != null)
                    foreach (var ps in previousScores)
                        if (!checkedIds.Contains(ps.ContentId) && (((double)ps.Score / (double)ps.Total) * 100 < 75))// if student scored less than 75 percent
                        {
                            //Before adding check if there are newer scores for this chapter with better result
                            if (previousScores.Any(x => ps.ContentId == x.ContentId && ((double)x.Score / (double)x.Total) * 100 >= 75 && x.testDate > ps.testDate))
                                continue;
                            // get for this content the questions and randomize them to take X random
                            var thisChapQuestions = content.Where(x => x.Id == ps.ContentId).SelectMany(x => x.Tests.ToList()).OrderBy(a => Guid.NewGuid()).ToList();

                            // add X amount of questions +3 to see if the student has improved!
                            finalsQ.AddRange(thisChapQuestions.Take(takeFromEachContent + 3));

                            //add the content ID to the checked ones to not add anymore 
                            checkedIds.Add(ps.ContentId);
                        }

                foreach (var i in content)
                {
                    if (!checkedIds.Contains(i.Id))
                    {
                        // add X amount of questions!
                        finalsQ.AddRange(i.Tests.OrderBy(a => Guid.NewGuid()).ToList().Take(takeFromEachContent));

                        //add the content ID to the checked ones to not add anymore 
                        checkedIds.Add(i.Id);
                    }
                }

                // Now that all contents have been passed (maybe), check the size of the array if bigger or smaller fix the size of the list
                if (finalsQ.Count > finalQuestionsAmount)
                { // if more remove X questions
                    var howMany = finalsQ.Count - finalQuestionsAmount;
                    finalsQ.RemoveRange(0, howMany);
                }

                else if (finalsQ.Count < finalQuestionsAmount)
                {// if less randomize all questions and take X
                    var howMany = finalQuestionsAmount - finalsQ.Count;
                    finalsQ.AddRange(content.SelectMany(x => x.Tests).OrderBy(a => Guid.NewGuid()).ToList().Take(howMany));
                }

                //shuffle the questions and store them to a temp data to later match them
                TempData["correctAnswers"] = finalsQ = finalsQ.OrderBy(a => Guid.NewGuid()).ToList();
            }
            #endregion

            return View(finalsQ);
        }

        [HttpPost]
        public void submitAnswersFinals(string values)
        {
            var chapter = (int)TempData["chapterID"];
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

            string chapName;
            using (var db = new SoftwareEduEntities())
            {
                chapName = db.Chapters.FirstOrDefault(x => x.Id == chapter).ChapterTitle;
                db.FinalsScores.Add(new FinalsScores
                {
                    Score = score,
                    chapId = chapter,
                    Total = correctAnswers.Count,
                    UserID = (int)Session["id"],
                    testDate = DateTime.Now,
                    Suggestion = ((double)score / (double)correctAnswers.Count) * 100 < 75 ? JsonConvert.SerializeObject(suggestions) : null
                });
                db.SaveChanges();
            }
            TempData["success"] = $"You have succesfully completed the final test for {chapName}, you can navigate to My Results to see how you did!";
        }

        public bool userCanDoFinal(int chapId)
        {
            var userId = (int)Session["id"];
            using (var db = new SoftwareEduEntities())
            {
                var chapterIds = db.Content.Where(x => x.chapId == chapId).Select(x => x.Id).ToList();
                var userScores = db.Scores
                    .Where(
                        x => x.UserID == userId &&
                            chapterIds.Contains(x.ContentId)
                    ).Select(x => x.ContentId).Distinct().Count();

                if (userScores == chapterIds.Count)
                    return true;
            }
            return false;
        }
        #endregion

        #region Scores methods
        public ActionResult MyScores(int? contentID = null, DateTime? selectedDate = null)
        {
            TempData["contentId"] = contentID;
            if (Session["id"] == null)
            {
                TempData["error"] = "Looks like you are not logged in..";
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
                        .Include(x => x.Content.Chapters)
                        .ToList(); // get scores for all chapters
                else
                    myScores = db.Scores
                        .Where(x => x.ContentId == contentID && x.UserID == userId)
                        .Include(x => x.Content)
                        .Include(x => x.Content.Chapters)
                        .ToList(); // get for specific chapter
            }

            if (selectedDate != null)
            {
                ViewData["datePick"] = selectedDate;
                myScores = myScores.Where(x => x.testDate.Date == selectedDate.Value.Date).ToList();
            }
            return View(myScores);
        }


        public ActionResult MyScoresFinals(int? chapId = null, DateTime? selectedDate = null)
        {
            TempData["contentId"] = chapId;
            if (Session["id"] == null)
            {
                TempData["error"] = "Looks like you are not logged in..";
                return RedirectToAction("Index", "Home");
            }

            var userId = (int)Session["id"];
            var myScores = new List<FinalsScores>();

            using (var db = new SoftwareEduEntities())
            {
                if (chapId == null)
                    myScores = db.FinalsScores
                        .Where(x => x.UserID == userId)
                        .Include(x => x.Chapters)
                        .ToList(); // get scores for all chapters
                else
                    myScores = db.FinalsScores
                        .Where(x => x.chapId == chapId && x.UserID == userId)
                        .Include(x => x.Chapters)
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