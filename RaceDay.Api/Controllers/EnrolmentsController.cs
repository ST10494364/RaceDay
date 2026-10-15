using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api/events/{eventId}/enrolments")]
    public class EnrolmentsController : ControllerBase
    {//start of class

        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {//start of constructor
            _context = context;
        }//end of constructor

        /// <summary>
        /// Enrols the logged-in Participant into an event under a chosen category. Participant only.
        /// </summary>
        /// <remarks>Body: category_id (must be a category of this event). A Participant can only enrol once per event.</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(int eventId, CreateEnrolmentRequest request)
        {//start of Enrol

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Participant")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Participants can enrol in events." });
            }//end of role check

            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
            {//start of event not found check
                return NotFound(new { message = "Event not found." });
            }//end of event not found check

            //400 if the category does not exist or belongs to a different event
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.EventId == eventId);
            if (category == null)
            {//start of category check
                return BadRequest(new { message = "That category does not belong to this event." });
            }//end of category check

            //409 if this Participant is already enrolled in this event
            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(en => en.ParticipantId == userId.Value && en.EventId == eventId);
            if (alreadyEnrolled)
            {//start of duplicate check
                return Conflict(new { message = "You are already enrolled in this event." });
            }//end of duplicate check

            var participant = await _context.Users.FindAsync(userId.Value);

            var enrolment = new Enrolment
            {//start of new enrolment
                ParticipantId = userId.Value,
                EventId = eventId,
                CategoryId = category.CategoryId
            };//end of new enrolment

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            enrolment.Participant = participant;
            enrolment.Category = category;
            return StatusCode(StatusCodes.Status201Created, ToResponse(enrolment));

        }//end of Enrol

        /// <summary>
        /// Returns all enrolments for an event. Organiser only, and only the Organiser who owns the event.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnrolments(int eventId)
        {//start of GetEnrolments

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can view enrolments." });
            }//end of role check

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (foundEvent == null)
            {//start of event not found check
                return NotFound(new { message = "Event not found." });
            }//end of event not found check

            //403 if this Organiser does not own the event
            if (foundEvent.OrganiserId != userId.Value)
            {//start of ownership check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view enrolments for your own events." });
            }//end of ownership check

            var enrolments = await _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.Category)
                .Where(en => en.EventId == eventId)
                .OrderBy(en => en.EnrolmentDate)
                .ToListAsync();

            return Ok(enrolments.Select(ToResponse).ToList());

        }//end of GetEnrolments

        //converts an Enrolment into the response shape
        private static EnrolmentResponse ToResponse(Enrolment en)
        {//start of ToResponse
            return new EnrolmentResponse
            {//start of response object
                EnrolmentId = en.EnrolmentId,
                ParticipantId = en.ParticipantId,
                ParticipantName = en.Participant?.FullName ?? string.Empty,
                EventId = en.EventId,
                CategoryId = en.CategoryId,
                CategoryName = en.Category?.CategoryName ?? string.Empty,
                EnrolmentDate = en.EnrolmentDate
            };//end of response object
        }//end of ToResponse

    }//end of class
}//end of namespace