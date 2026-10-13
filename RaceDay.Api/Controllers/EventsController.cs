using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {//start of class

        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {//start of constructor
            _context = context;
        }//end of constructor

        /// <summary>
        /// Creates a new event owned by the logged-in Organiser. Organiser only.
        /// </summary>
        /// <remarks>Body: name, description, event_date, location, distance_km, event_type_id (1 = Run, 2 = Walk, 3 = Cycle).</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent(CreateEventRequest request)
        {//start of CreateEvent

            //401 if nobody is logged in
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            //403 if the logged-in user is not an Organiser
            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can create events." });
            }//end of role check

            //400 if the event type does not exist
            var eventType = await _context.EventTypes.FindAsync(request.EventTypeId);
            if (eventType == null)
            {//start of event type check
                return BadRequest(new { message = "Event type does not exist." });
            }//end of event type check

            var newEvent = new Event
            {//start of new event
                OrganiserId = userId.Value,
                EventTypeId = request.EventTypeId,
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                EventDate = request.EventDate.Date,
                Location = request.Location.Trim(),
                DistanceKm = request.DistanceKm,
                Status = "Open"
            };//end of new event

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            newEvent.EventType = eventType;
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, ToResponse(newEvent));

        }//end of CreateEvent

        /// <summary>
        /// Returns all events. Any logged-in role.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllEvents()
        {//start of GetAllEvents

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            var events = await _context.Events
                .Include(e => e.EventType)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(ToResponse).ToList());

        }//end of GetAllEvents

        /// <summary>
        /// Returns the details of one event. Any logged-in role.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(int id)
        {//start of GetEventById

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            var foundEvent = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (foundEvent == null)
            {//start of not found check
                return NotFound(new { message = "Event not found." });
            }//end of not found check

            return Ok(ToResponse(foundEvent));

        }//end of GetEventById

        //converts an Event into the response shape
        private static EventResponse ToResponse(Event e)
        {//start of ToResponse
            return new EventResponse
            {//start of response object
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                EventTypeId = e.EventTypeId,
                EventTypeName = e.EventType?.TypeName ?? string.Empty,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                Status = e.Status
            };//end of response object
        }//end of ToResponse

    }//end of class
}//end of namespace