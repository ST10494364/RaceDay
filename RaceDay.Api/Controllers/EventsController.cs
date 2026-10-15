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

        /// <summary>
        /// Updates an existing event. Organiser only, and only the Organiser who owns the event.
        /// </summary>
        /// <remarks>Body: name, description, event_date, location, distance_km, event_type_id, status (Open, Closed, Completed or Cancelled).</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, UpdateEventRequest request)
        {//start of UpdateEvent

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can update events." });
            }//end of role check

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (foundEvent == null)
            {//start of not found check
                return NotFound(new { message = "Event not found." });
            }//end of not found check

            //403 if this Organiser does not own the event
            if (foundEvent.OrganiserId != userId.Value)
            {//start of ownership check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only update your own events." });
            }//end of ownership check

            var eventType = await _context.EventTypes.FindAsync(request.EventTypeId);
            if (eventType == null)
            {//start of event type check
                return BadRequest(new { message = "Event type does not exist." });
            }//end of event type check

            foundEvent.Name = request.Name.Trim();
            foundEvent.Description = request.Description.Trim();
            foundEvent.EventDate = request.EventDate.Date;
            foundEvent.Location = request.Location.Trim();
            foundEvent.DistanceKm = request.DistanceKm;
            foundEvent.EventTypeId = request.EventTypeId;
            foundEvent.Status = request.Status;
            await _context.SaveChangesAsync();

            foundEvent.EventType = eventType;
            return Ok(ToResponse(foundEvent));

        }//end of UpdateEvent

        /// <summary>
        /// Deletes an event together with its categories, enrolments and results. Organiser only, and only the Organiser who owns the event.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {//start of DeleteEvent

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can delete events." });
            }//end of role check

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (foundEvent == null)
            {//start of not found check
                return NotFound(new { message = "Event not found." });
            }//end of not found check

            if (foundEvent.OrganiserId != userId.Value)
            {//start of ownership check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only delete your own events." });
            }//end of ownership check

            //the database does not cascade deletes, so remove the child rows first
            //EF Core saves these deletes together in one transaction
            _context.Results.RemoveRange(_context.Results.Where(r => r.EventId == id));
            _context.Enrolments.RemoveRange(_context.Enrolments.Where(en => en.EventId == id));
            _context.Categories.RemoveRange(_context.Categories.Where(c => c.EventId == id));
            _context.Events.Remove(foundEvent);
            await _context.SaveChangesAsync();

            return NoContent();

        }//end of DeleteEvent
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