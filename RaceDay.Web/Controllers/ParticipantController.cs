using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //the Participant's own events and race history
    public class ParticipantController : Controller
    {//start of class

        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        //sends visitors to log in and keeps Organisers out (null means the user is a Participant)
        private IActionResult? RequireParticipant()
        {//start of RequireParticipant

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            if (HttpContext.Session.GetString("Role") != "Participant")
            {//start of role check
                TempData["Problem"] = "That page is only for Participants.";
                return RedirectToAction("Index", "Home");
            }//end of role check

            return null;

        }//end of RequireParticipant

        //every event the Participant has enrolled for, with the enrolment status
        public async Task<IActionResult> MyEvents()
        {//start of MyEvents

            var guard = RequireParticipant();
            if (guard != null) return guard;

            var enrolments = await _api.GetAsync<List<EnrolmentDto>>("api/enrolments/me");
            if (!enrolments.Success)
            {//start of load error check
                TempData["Problem"] = enrolments.Message;
            }//end of load error check

            //the enrolment only holds the event id, so look the events up to show names and dates
            var events = await _api.GetAsync<List<EventDto>>("api/events");
            var lookup = (events.Data ?? new List<EventDto>()).ToDictionary(e => e.EventId);

            var rows = (enrolments.Data ?? new List<EnrolmentDto>())
                .Select(en => new MyEventRow { Enrolment = en, Event = lookup.GetValueOrDefault(en.EventId) })
                .OrderBy(r => r.Event?.EventDate)
                .ToList();

            return View(rows);

        }//end of MyEvents

        //the Participant's race history with finish times and positions
        public async Task<IActionResult> MyResults()
        {//start of MyResults

            var guard = RequireParticipant();
            if (guard != null) return guard;

            var results = await _api.GetAsync<List<ResultDto>>("api/results/me");
            if (!results.Success)
            {//start of load error check
                TempData["Problem"] = results.Message;
            }//end of load error check

            return View((results.Data ?? new List<ResultDto>()).OrderByDescending(r => r.EventDate).ToList());

        }//end of MyResults

    }//end of class
}//end of namespace