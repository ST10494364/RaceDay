using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class CategoriesController : ControllerBase
    {//start of class

        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {//start of constructor
            _context = context;
        }//end of constructor

        /// <summary>
        /// Creates a new category for an event. Organiser only, and only the Organiser who owns the event.
        /// </summary>
        /// <remarks>Body: category_name (for example "10km Open" or "5km Walk").</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCategory(int eventId, CreateCategoryRequest request)
        {//start of CreateCategory

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can create categories." });
            }//end of role check

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (foundEvent == null)
            {//start of event not found check
                return NotFound(new { message = "Event not found." });
            }//end of event not found check

            //403 if this Organiser does not own the event
            if (foundEvent.OrganiserId != userId.Value)
            {//start of ownership check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only add categories to your own events." });
            }//end of ownership check

            var category = new Category
            {//start of new category
                EventId = eventId,
                CategoryName = request.CategoryName.Trim()
            };//end of new category

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, ToResponse(category));

        }//end of CreateCategory

        /// <summary>
        /// Returns all categories for an event. Any logged-in role.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategories(int eventId)
        {//start of GetCategories

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
            {//start of event not found check
                return NotFound(new { message = "Event not found." });
            }//end of event not found check

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return Ok(categories.Select(ToResponse).ToList());

        }//end of GetCategories

        //converts a Category into the response shape
        private static CategoryResponse ToResponse(Category c)
        {//start of ToResponse
            return new CategoryResponse
            {//start of response object
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                CategoryName = c.CategoryName
            };//end of response object
        }//end of ToResponse

    }//end of class
}//end of namespace