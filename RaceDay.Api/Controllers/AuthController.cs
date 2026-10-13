using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {//start of class

        private readonly RaceDayContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthController(RaceDayContext context)
        {//start of constructor
            _context = context;
        }//end of constructor

        /// <summary>
        /// Creates a new user account (Organiser or Participant). Public endpoint.
        /// </summary>
        /// <remarks>Body: full_name, email, password (min 8 characters), role (Organiser or Participant).</remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {//start of Register

            //emails are stored in lower case so duplicates are caught reliably
            var email = request.Email.Trim().ToLower();

            //409 if the email is already registered
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists)
            {//start of email exists check
                return Conflict(new { message = "An account with this email already exists." });
            }//end of email exists check

            var user = new User
            {//start of new user
                FullName = request.FullName.Trim(),
                Email = email,
                Role = request.Role
            };//end of new user

            //hash the password before saving, the plain password is never stored
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, ToResponse(user));

        }//end of Register

        /// <summary>
        /// Logs a user in and starts a server-side session that stores their user ID and role. Public endpoint.
        /// </summary>
        /// <remarks>Body: email, password. Returns the user's details; the session cookie is set automatically.</remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request)
        {//start of Login

            var email = request.Email.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            //same message for unknown email and wrong password, so attackers learn nothing
            if (user == null)
            {//start of unknown email check
                return Unauthorized(new { message = "Invalid email or password." });
            }//end of unknown email check

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {//start of wrong password check
                return Unauthorized(new { message = "Invalid email or password." });
            }//end of wrong password check

            //store the user ID and role in the server-side session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(ToResponse(user));

        }//end of Login

        //converts a User into the safe response shape (no password hash)
        private static UserResponse ToResponse(User user)
        {//start of ToResponse
            return new UserResponse
            {//start of response object
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };//end of response object
        }//end of ToResponse

    }//end of class
}//end of namespace