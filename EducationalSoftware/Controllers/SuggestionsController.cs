using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalSoftware.Controllers
{
    public class SuggestionsController : Controller
    {
        // GET: Suggestions
        public ActionResult SeeSuggestion(int? testId)
        {

            return View();
        }
    }
}