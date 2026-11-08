using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {//start of class

        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {//start of constructor
            _context = context;
        }//end of constructor

        /// <summary>
        /// Captures a Participant's finish time and position for an event. Organiser only, and only the Organiser who owns the event.
        /// </summary>
        /// <remarks>Body: participant_id, finish_time (hh:mm:ss), finishing_position. The Participant must be enrolled in the event and can only have one result per event.</remarks>
        [HttpPost("events/{eventId}/results")]
        [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateResult(int eventId, CreateResultRequest request)
        {//start of CreateResult

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can capture results." });
            }//end of role check

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (foundEvent == null)
            {//start of event not found check
                return NotFound(new { message = "Event not found." });
            }//end of event not found check

            //403 if this Organiser does not own the event
            if (foundEvent.OrganiserId != userId.Value)
            {//start of ownership check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only capture results for your own events." });
            }//end of ownership check

            //400 if the finish time is not a positive time
            if (request.FinishTime <= TimeSpan.Zero)
            {//start of finish time check
                return BadRequest(new { message = "Finish time must be greater than zero." });
            }//end of finish time check

            //400 if this Participant is not enrolled in the event
            bool isEnrolled = await _context.Enrolments
                .AnyAsync(en => en.EventId == eventId && en.ParticipantId == request.ParticipantId);
            if (!isEnrolled)
            {//start of enrolled check
                return BadRequest(new { message = "That participant is not enrolled in this event." });
            }//end of enrolled check

            //400 if a result was already captured for this Participant and event
            bool resultExists = await _context.Results
                .AnyAsync(r => r.EventId == eventId && r.ParticipantId == request.ParticipantId);
            if (resultExists)
            {//start of duplicate check
                return BadRequest(new { message = "A result has already been captured for this participant." });
            }//end of duplicate check

            var result = new Result
            {//start of new result
                ParticipantId = request.ParticipantId,
                EventId = eventId,
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition
            };//end of new result

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            result.Event = foundEvent;
            return StatusCode(StatusCodes.Status201Created, ToResponse(result));

        }//end of CreateResult

        /// <summary>
        /// Returns the logged-in Participant's own results across all events. Participant only.
        /// </summary>
        [HttpGet("results/me")]
        [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {//start of GetMyResults

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Participant")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Participants have personal results." });
            }//end of role check

            var results = await _context.Results
                .Include(r => r.Event)
                .Where(r => r.ParticipantId == userId.Value)
                .OrderBy(r => r.Event!.EventDate)
                .ToListAsync();

            //ids of the events these results belong to
            var eventIds = results.Select(r => r.EventId).Distinct().ToList();

            //how many Participants finished each of those events
            var finisherCounts = await _context.Results
                .Where(r => eventIds.Contains(r.EventId))
                .GroupBy(r => r.EventId)
                .Select(g => new { EventId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(g => g.EventId, g => g.Total);

            //the category this Participant entered each event in
            var categoryNames = await _context.Enrolments
                .Include(en => en.Category)
                .Where(en => en.ParticipantId == userId.Value && eventIds.Contains(en.EventId))
                .ToDictionaryAsync(en => en.EventId, en => en.Category!.CategoryName);

            //build each response and add the finisher total and category name
            var response = results.Select(r =>
            {//start of response builder
                var item = ToResponse(r);
                item.TotalFinishers = finisherCounts.GetValueOrDefault(r.EventId);
                item.CategoryName = categoryNames.GetValueOrDefault(r.EventId, string.Empty);
                return item;
            }).ToList();//end of response builder

            return Ok(response);
        }//end of GetMyResults

        //converts a Result into the response shape
        private static ResultResponse ToResponse(Result r)
        {//start of ToResponse
            return new ResultResponse
            {//start of response object
                ResultId = r.ResultId,
                ParticipantId = r.ParticipantId,
                EventId = r.EventId,
                EventName = r.Event?.Name ?? string.Empty,
                EventDate = r.Event?.EventDate ?? default,
                FinishTime = r.FinishTime,
                FinishingPosition = r.FinishingPosition
            };//end of response object
        }//end of ToResponse

    }//end of class
}//end of namespace