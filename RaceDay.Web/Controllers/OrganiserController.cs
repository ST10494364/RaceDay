using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //everything an Organiser can do: dashboard, events, categories, enrolments and results
    public class OrganiserController : Controller
    {//start of class

        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        //sends visitors to log in and keeps Participants out (null means the user is an Organiser)
        private IActionResult? RequireOrganiser()
        {//start of RequireOrganiser

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                TempData["Problem"] = "That page is only for Organisers.";
                return RedirectToAction("Index", "Home");
            }//end of role check

            return null;

        }//end of RequireOrganiser

        //the events created by the logged-in Organiser, soonest first
        private async Task<List<EventDto>> LoadMyEventsAsync()
        {//start of LoadMyEventsAsync

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var result = await _api.GetAsync<List<EventDto>>("api/events");

            if (!result.Success)
            {//start of load error check
                TempData["Problem"] = result.Message;
            }//end of load error check

            return (result.Data ?? new List<EventDto>())
                .Where(e => e.OrganiserId == userId)
                .OrderBy(e => e.EventDate)
                .ToList();

        }//end of LoadMyEventsAsync

        //the JSON body the API expects when creating or updating an event
        private static object ToBody(EventFormViewModel m)
        {//start of ToBody
            return new
            {
                name = m.Name.Trim(),
                description = m.Description.Trim(),
                event_date = m.EventDate.ToString("yyyy-MM-dd"),
                location = m.Location.Trim(),
                distance_km = m.DistanceKm,
                event_type_id = m.EventTypeId,
                status = m.Status
            };
        }//end of ToBody

        //summary of the Organiser's events with enrolment totals
        public async Task<IActionResult> Dashboard()
        {//start of Dashboard

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            var events = await LoadMyEventsAsync();
            var model = new DashboardViewModel();

            foreach (var e in events)
            {//start of event loop
                var enrolments = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{e.EventId}/enrolments");
                model.Rows.Add(new DashboardRow { Event = e, TotalEnrolments = enrolments.Data?.Count ?? 0 });
            }//end of event loop

            model.TotalEvents = events.Count;
            model.UpcomingEvents = events.Count(e => e.EventDate.Date >= DateTime.Today);
            model.TotalEnrolments = model.Rows.Sum(r => r.TotalEnrolments);

            return View(model);

        }//end of Dashboard

        //the events management page
        public async Task<IActionResult> Events()
        {//start of Events

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            return View(await LoadMyEventsAsync());

        }//end of Events

        [HttpGet]
        public IActionResult Create()
        {//start of Create (GET)

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            return View("EventForm", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30) });

        }//end of Create (GET)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {//start of Create (POST)

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            if (!ModelState.IsValid)
            {//start of invalid form check
                return View("EventForm", model);
            }//end of invalid form check

            var result = await _api.PostAsync<EventDto>("api/events", ToBody(model));
            if (!result.Success || result.Data == null)
            {//start of API error check
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }//end of API error check

            //the banner goes to the API, which stores it in Azure Blob Storage
            if (model.Banner != null && model.Banner.Length > 0)
            {//start of banner upload
                var upload = await _api.UploadFileAsync($"api/events/{result.Data.EventId}/banner", model.Banner);
                if (!upload.Success)
                {//start of upload error check
                    TempData["Problem"] = "The event was created, but the banner could not be uploaded: " + upload.Message;
                    return RedirectToAction("Events");
                }//end of upload error check
            }//end of banner upload

            TempData["Notice"] = "Event created.";
            return RedirectToAction("Events");

        }//end of Create (POST)

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {//start of Edit (GET)

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var result = await _api.GetAsync<EventDto>($"api/events/{id}");

            if (!result.Success || result.Data == null || result.Data.OrganiserId != userId)
            {//start of not found or not owner check
                TempData["Problem"] = "That event could not be found among your events.";
                return RedirectToAction("Events");
            }//end of not found or not owner check

            var e = result.Data;
            return View("EventForm", new EventFormViewModel
            {
                EventId = e.EventId,
                IsEdit = true,
                HasBanner = !string.IsNullOrEmpty(e.BannerImageUrl),
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventTypeId = e.EventTypeId,
                Status = e.Status
            });

        }//end of Edit (GET)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventFormViewModel model)
        {//start of Edit (POST)

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            model.IsEdit = true;
            if (!ModelState.IsValid)
            {//start of invalid form check
                return View("EventForm", model);
            }//end of invalid form check

            var result = await _api.PutAsync<EventDto>($"api/events/{model.EventId}", ToBody(model));
            if (!result.Success)
            {//start of API error check
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }//end of API error check

            if (model.Banner != null && model.Banner.Length > 0)
            {//start of banner upload
                var upload = await _api.UploadFileAsync($"api/events/{model.EventId}/banner", model.Banner);
                if (!upload.Success)
                {//start of upload error check
                    TempData["Problem"] = "The event was saved, but the banner could not be uploaded: " + upload.Message;
                    return RedirectToAction("Events");
                }//end of upload error check
            }//end of banner upload

            TempData["Notice"] = "Event updated.";
            return RedirectToAction("Events");

        }//end of Edit (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {//start of Delete

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            var result = await _api.DeleteAsync($"api/events/{id}");

            if (result.Success)
            {//start of success branch
                TempData["Notice"] = "Event deleted.";
            }//end of success branch
            else
            {//start of failure branch
                TempData["Problem"] = result.Message;
            }//end of failure branch

            return RedirectToAction("Events");

        }//end of Delete

        //the event with this id, but only if it belongs to the logged-in Organiser (otherwise null)
        private async Task<EventDto?> LoadOwnedEventAsync(int id)
        {//start of LoadOwnedEventAsync

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var result = await _api.GetAsync<EventDto>($"api/events/{id}");

            if (!result.Success || result.Data == null || result.Data.OrganiserId != userId)
            {//start of not found or not owner check
                TempData["Problem"] = "That event could not be found among your events.";
                return null;
            }//end of not found or not owner check

            return result.Data;

        }//end of LoadOwnedEventAsync

        //the categories of an event, and a form to add one
        public async Task<IActionResult> Categories(int id)
        {//start of Categories

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            var ev = await LoadOwnedEventAsync(id);
            if (ev == null) return RedirectToAction("Events");

            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");
            return View(new EventWorkViewModel { Event = ev, Categories = categories.Data ?? new List<CategoryDto>() });

        }//end of Categories

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int id, string categoryName)
        {//start of AddCategory

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            if (string.IsNullOrWhiteSpace(categoryName))
            {//start of empty name check
                TempData["Problem"] = "Please enter a category name, for example 10 km or Under 18.";
                return RedirectToAction("Categories", new { id });
            }//end of empty name check

            var result = await _api.PostAsync<CategoryDto>($"api/events/{id}/categories", new { category_name = categoryName.Trim() });

            if (result.Success)
            {//start of success branch
                TempData["Notice"] = "Category added.";
            }//end of success branch
            else
            {//start of failure branch
                TempData["Problem"] = result.Message;
            }//end of failure branch

            return RedirectToAction("Categories", new { id });

        }//end of AddCategory

        //everyone who has enrolled for an event, with their category and status
        public async Task<IActionResult> Enrolments(int id)
        {//start of Enrolments

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            var ev = await LoadOwnedEventAsync(id);
            if (ev == null) return RedirectToAction("Events");

            var enrolments = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");
            return View(new EventWorkViewModel { Event = ev, Enrolments = enrolments.Data ?? new List<EnrolmentDto>() });

        }//end of Enrolments

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEnrolment(int id, int enrolmentId)
        {//start of ConfirmEnrolment

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            var result = await _api.PutAsync<EnrolmentDto>($"api/events/{id}/enrolments/{enrolmentId}/confirm", null);

            if (result.Success)
            {//start of success branch
                TempData["Notice"] = "Enrolment confirmed.";
            }//end of success branch
            else
            {//start of failure branch
                TempData["Problem"] = result.Message;
            }//end of failure branch

            return RedirectToAction("Enrolments", new { id });

        }//end of ConfirmEnrolment

        //the results entry page: one row per enrolled Participant
        public async Task<IActionResult> Results(int id)
        {//start of Results

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            var ev = await LoadOwnedEventAsync(id);
            if (ev == null) return RedirectToAction("Events");

            var enrolments = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");
            return View(new EventWorkViewModel { Event = ev, Enrolments = enrolments.Data ?? new List<EnrolmentDto>() });

        }//end of Results

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int id, int participantId, string finishTime, int position)
        {//start of SaveResult

            var guard = RequireOrganiser();
            if (guard != null) return guard;

            if (!TimeSpan.TryParse(finishTime, out TimeSpan time) || time <= TimeSpan.Zero)
            {//start of time check
                TempData["Problem"] = "Please enter the finish time as hours:minutes:seconds, for example 00:52:30.";
                return RedirectToAction("Results", new { id });
            }//end of time check

            if (position < 1)
            {//start of position check
                TempData["Problem"] = "The finishing position must be 1 or higher.";
                return RedirectToAction("Results", new { id });
            }//end of position check

            var result = await _api.PostAsync<object>($"api/events/{id}/results", new
            {
                participant_id = participantId,
                finish_time = time.ToString(@"hh\:mm\:ss"),
                finishing_position = position
            });

            if (result.Success)
            {//start of success branch
                TempData["Notice"] = "Result saved.";
            }//end of success branch
            else
            {//start of failure branch
                TempData["Problem"] = result.Message;
            }//end of failure branch

            return RedirectToAction("Results", new { id });

        }//end of SaveResult


    }//end of class
}//end of namespace