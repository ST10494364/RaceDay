using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //event details and entering an event
    public class EventsController : Controller
    {//start of class

        private readonly ApiClient _api;

        public EventsController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        //full information about one event, with its categories and the Participant's own enrolment
        public async Task<IActionResult> Details(int id)
        {//start of Details

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            var eventResult = await _api.GetAsync<EventDto>($"api/events/{id}");
            if (!eventResult.Success || eventResult.Data == null)
            {//start of event error check
                TempData["Problem"] = eventResult.Message;
                return RedirectToAction("Index", "Home");
            }//end of event error check

            var categoryResult = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");

            //a Participant can see whether they are already enrolled in this event
            EnrolmentDto? mine = null;
            if (HttpContext.Session.GetString("Role") == "Participant")
            {//start of own enrolment lookup
                var enrolments = await _api.GetAsync<List<EnrolmentDto>>("api/enrolments/me");
                mine = enrolments.Data?.FirstOrDefault(e => e.EventId == id);
            }//end of own enrolment lookup

            return View(new EventDetailsViewModel
            {
                Event = eventResult.Data,
                Categories = categoryResult.Data ?? new List<CategoryDto>(),
                MyEnrolment = mine
            });

        }//end of Details

        //enrols the logged-in Participant in the event under the chosen category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int id, int categoryId)
        {//start of Enrol

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            if (HttpContext.Session.GetString("Role") != "Participant")
            {//start of role check
                TempData["Problem"] = "Only Participants can enter events.";
                return RedirectToAction("Details", new { id });
            }//end of role check

            if (categoryId == 0)
            {//start of category check
                TempData["Problem"] = "Please choose a category before entering.";
                return RedirectToAction("Details", new { id });
            }//end of category check

            var result = await _api.PostAsync<EnrolmentDto>($"api/events/{id}/enrolments", new { category_id = categoryId });

            if (result.Success)
            {//start of success branch
                TempData["Notice"] = "You are entered! Your enrolment is pending until the Organiser confirms it.";
            }//end of success branch
            else
            {//start of failure branch
                TempData["Problem"] = result.Message;
            }//end of failure branch

            return RedirectToAction("Details", new { id });

        }//end of Enrol

    }//end of class
}//end of namespace