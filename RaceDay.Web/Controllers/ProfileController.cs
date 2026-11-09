using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //view and update personal information and upload a profile picture (both roles)
    public class ProfileController : Controller
    {//start of class

        private readonly ApiClient _api;

        public ProfileController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        public async Task<IActionResult> Index()
        {//start of Index

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            var result = await _api.GetAsync<UserDto>("api/users/me");
            if (!result.Success || result.Data == null)
            {//start of load error check
                TempData["Problem"] = result.Message;
                return RedirectToAction("Index", "Home");
            }//end of load error check

            return View(new ProfileViewModel
            {
                UserId = result.Data.UserId,
                Role = result.Data.Role,
                HasPicture = !string.IsNullOrEmpty(result.Data.ProfilePictureUrl),
                FullName = result.Data.FullName,
                Email = result.Data.Email
            });

        }//end of Index

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {//start of Update

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            if (!ModelState.IsValid)
            {//start of invalid form check
                return View("Index", model);
            }//end of invalid form check

            var result = await _api.PutAsync<UserDto>("api/users/me", new
            {
                full_name = model.FullName.Trim(),
                email = model.Email.Trim()
            });

            if (!result.Success || result.Data == null)
            {//start of API error check
                ModelState.AddModelError(string.Empty, result.Message);
                return View("Index", model);
            }//end of API error check

            //keep the name in the menu up to date
            HttpContext.Session.SetString("FullName", result.Data.FullName);

            TempData["Notice"] = "Your details have been updated.";
            return RedirectToAction("Index");

        }//end of Update

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {//start of UploadPicture

            if (HttpContext.Session.GetInt32("UserId") == null)
            {//start of login check
                return RedirectToAction("Login", "Account");
            }//end of login check

            if (picture == null || picture.Length == 0)
            {//start of no file check
                TempData["Problem"] = "Please choose an image first.";
                return RedirectToAction("Index");
            }//end of no file check

            //the file goes to the API, which stores it in Azure Blob Storage
            var result = await _api.UploadFileAsync("api/users/me/profile-picture", picture);

            if (result.Success)
            {//start of success branch
                TempData["Notice"] = "Profile picture updated.";
            }//end of success branch
            else
            {//start of failure branch
                TempData["Problem"] = result.Message;
            }//end of failure branch

            return RedirectToAction("Index");

        }//end of UploadPicture

    }//end of class
}//end of namespace