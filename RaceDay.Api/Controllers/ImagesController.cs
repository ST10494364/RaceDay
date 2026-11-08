using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api")]
    public class ImagesController : ControllerBase
    {//start of class

        //only these image types are accepted, and no file may be larger than 5 MB
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private readonly RaceDayContext _context;
        private readonly IBlobStorageService _blobStorage;

        public ImagesController(RaceDayContext context, IBlobStorageService blobStorage)
        {//start of constructor
            _context = context;
            _blobStorage = blobStorage;
        }//end of constructor

        /// <summary>
        /// Uploads a banner image for an event and saves its Azure address on the event. Organiser only, and only the Organiser who owns the event.
        /// </summary>
        /// <remarks>Send the image as form-data in a field called file. JPG, PNG or WEBP, 5 MB at most.</remarks>
        [HttpPost("events/{eventId}/banner")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadEventBanner(int eventId, IFormFile file)
        {//start of UploadEventBanner

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {//start of role check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organisers can upload event banners." });
            }//end of role check

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (foundEvent == null)
            {//start of event not found check
                return NotFound(new { message = "Event not found." });
            }//end of event not found check

            //403 if this Organiser does not own the event
            if (foundEvent.OrganiserId != userId.Value)
            {//start of ownership check
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only upload banners for your own events." });
            }//end of ownership check

            string? problem = CheckImage(file);
            if (problem != null)
            {//start of image check
                return BadRequest(new { message = problem });
            }//end of image check

            //store the file in Azure and save its address on the event
            foundEvent.BannerImageUrl = await _blobStorage.UploadImageAsync(file, "events");
            await _context.SaveChangesAsync();

            return Ok(new { message = "Banner uploaded.", banner_image_url = foundEvent.BannerImageUrl });

        }//end of UploadEventBanner

        /// <summary>
        /// Returns the banner image of an event. Open to everyone, because event pages are public.
        /// </summary>
        [HttpGet("events/{eventId}/banner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventBanner(int eventId)
        {//start of GetEventBanner

            var foundEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (foundEvent == null || string.IsNullOrEmpty(foundEvent.BannerImageUrl))
            {//start of no banner check
                return NotFound(new { message = "This event has no banner image." });
            }//end of no banner check

            var image = await _blobStorage.GetImageAsync(foundEvent.BannerImageUrl);
            if (image == null)
            {//start of image missing check
                return NotFound(new { message = "The banner image could not be found." });
            }//end of image missing check

            return File(image.Value.Content, image.Value.ContentType);

        }//end of GetEventBanner

        /// <summary>
        /// Uploads a profile picture for the logged-in user and saves its Azure address on their account. Any logged-in user.
        /// </summary>
        /// <remarks>Send the image as form-data in a field called file. JPG, PNG or WEBP, 5 MB at most.</remarks>
        [HttpPost("users/me/profile-picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {//start of UploadProfilePicture

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {//start of user not found check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of user not found check

            string? problem = CheckImage(file);
            if (problem != null)
            {//start of image check
                return BadRequest(new { message = problem });
            }//end of image check

            //store the file in Azure and save its address on the user
            user.ProfilePictureUrl = await _blobStorage.UploadImageAsync(file, "profiles");
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture uploaded.", profile_picture_url = user.ProfilePictureUrl });

        }//end of UploadProfilePicture

        /// <summary>
        /// Returns a user's profile picture. Any logged-in user.
        /// </summary>
        [HttpGet("users/{userId}/profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfilePicture(int userId)
        {//start of GetProfilePicture

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of not logged in check
                return Unauthorized(new { message = "You must be logged in." });
            }//end of not logged in check

            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ProfilePictureUrl))
            {//start of no picture check
                return NotFound(new { message = "This user has no profile picture." });
            }//end of no picture check

            var image = await _blobStorage.GetImageAsync(user.ProfilePictureUrl);
            if (image == null)
            {//start of image missing check
                return NotFound(new { message = "The profile picture could not be found." });
            }//end of image missing check

            return File(image.Value.Content, image.Value.ContentType);

        }//end of GetProfilePicture

        //checks that a file was sent, is an allowed image type and is not too large (null means it is fine)
        private static string? CheckImage(IFormFile? file)
        {//start of CheckImage

            if (file == null || file.Length == 0)
            {//start of empty file check
                return "Please choose an image file.";
            }//end of empty file check

            if (file.Length > MaxFileSizeBytes)
            {//start of size check
                return "The image must be 5 MB or smaller.";
            }//end of size check

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {//start of type check
                return "Only JPG, PNG or WEBP images are allowed.";
            }//end of type check

            return null;

        }//end of CheckImage

    }//end of class
}//end of namespace