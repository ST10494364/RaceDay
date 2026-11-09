using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //passes images from the API to the browser, because the Azure container is private and only the API can read it
    public class ImagesController : Controller
    {//start of class

        private readonly ApiClient _api;

        public ImagesController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        public async Task<IActionResult> EventBanner(int id)
        {//start of EventBanner

            var image = await _api.GetImageAsync($"api/events/{id}/banner");
            if (image == null)
            {//start of no image check
                return NotFound();
            }//end of no image check

            return File(image.Value.Bytes, image.Value.ContentType);

        }//end of EventBanner

        public async Task<IActionResult> ProfilePicture(int id)
        {//start of ProfilePicture

            var image = await _api.GetImageAsync($"api/users/{id}/profile-picture");
            if (image == null)
            {//start of no image check
                return NotFound();
            }//end of no image check

            return File(image.Value.Bytes, image.Value.ContentType);

        }//end of ProfilePicture

    }//end of class
}//end of namespace