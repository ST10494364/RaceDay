using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {//start of class

        private readonly RaceDayContext _context;

        public UsersController(RaceDayContext context)
        {//start of constructor
            _context = context;
        }//end of constructor

        /// <summary>
        /// Returns the profile of the logged-in user. Any logged-in role.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {//start of GetMyProfile

            //read the user ID from the server-side session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {//start of user missing check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of user missing check

            return Ok(new UserResponse
            {//start of response object
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });//end of response object

        }//end of GetMyProfile

        /// <summary>
        /// Updates the full name and email of the logged-in user. Any logged-in role.
        /// </summary>
        /// <remarks>Body: full_name, email. The role cannot be changed here.</remarks>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
        {//start of UpdateMyProfile

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {//start of user missing check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of user missing check

            var email = request.Email.Trim().ToLower();

            //the new email must not belong to a different user
            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != user.UserId);
            if (emailTaken)
            {//start of email taken check
                return BadRequest(new { message = "That email is already in use." });
            }//end of email taken check

            user.FullName = request.FullName.Trim();
            user.Email = email;
            await _context.SaveChangesAsync();

            return Ok(new UserResponse
            {//start of response object
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });//end of response object

        }//end of UpdateMyProfile

    }//end of class
}//end of namespace