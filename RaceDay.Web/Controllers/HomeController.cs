using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Diagnostics;

namespace RaceDay.Web.Controllers
{//start of namespace

    public class HomeController : Controller
    {//start of class

        private readonly ApiClient _api;

        public HomeController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        //upcoming events with search and type filters (visitors who are not logged in see the welcome page)
        public async Task<IActionResult> Index(string? search, string? type)
        {//start of Index

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of visitor check
                return View("Welcome");
            }//end of visitor check

            var result = await _api.GetAsync<List<EventDto>>("api/events");

            if (result.StatusCode == 401)
            {//start of expired login check
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }//end of expired login check

            if (!result.Success)
            {//start of load error check
                TempData["Problem"] = result.Message;
            }//end of load error check

            //only events that have not happened yet, soonest first
            var events = (result.Data ?? new List<EventDto>())
                .Where(e => e.EventDate.Date >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {//start of search filter
                string term = search.Trim();
                events = events.Where(e => e.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                                        || e.Location.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            }//end of search filter

            if (!string.IsNullOrWhiteSpace(type))
            {//start of type filter
                events = events.Where(e => e.EventTypeName == type).ToList();
            }//end of type filter

            return View(new HomeViewModel { Events = events, Search = search, Type = type });

        }//end of Index

        public IActionResult Privacy()
        {//start of Privacy
            return View();
        }//end of Privacy

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {//start of Error
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }//end of Error

    }//end of class
}//end of namespace