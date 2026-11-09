using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //handles creating an account, logging in and logging out
    public class AccountController : Controller
    {//start of class

        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {//start of constructor
            _api = api;
        }//end of constructor

        [HttpGet]
        public IActionResult Register()
        {//start of Register (GET)

            //someone who is already logged in has no need for this page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {//start of logged in check
                return RedirectToAction("Index", "Home");
            }//end of logged in check

            return View(new RegisterViewModel());

        }//end of Register (GET)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {//start of Register (POST)

            if (!ModelState.IsValid)
            {//start of invalid form check
                return View(model);
            }//end of invalid form check

            //the API creates the account, the MVC app never touches the database
            var result = await _api.PostAsync<UserDto>("api/auth/register", new
            {
                full_name = model.FullName.Trim(),
                email = model.Email.Trim(),
                password = model.Password,
                role = model.Role
            });

            if (!result.Success)
            {//start of API error check
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }//end of API error check

            TempData["SuccessMessage"] = "Your account has been created. Please log in.";
            return RedirectToAction("Login");

        }//end of Register (POST)

        [HttpGet]
        public IActionResult Login()
        {//start of Login (GET)

            if (HttpContext.Session.GetInt32("UserId") != null)
            {//start of logged in check
                return RedirectToAction("Index", "Home");
            }//end of logged in check

            return View(new LoginViewModel());

        }//end of Login (GET)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {//start of Login (POST)

            if (!ModelState.IsValid)
            {//start of invalid form check
                return View(model);
            }//end of invalid form check

            var result = await _api.PostAsync<UserDto>("api/auth/login", new
            {
                email = model.Email.Trim(),
                password = model.Password
            });

            if (!result.Success || result.Data == null)
            {//start of API error check
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }//end of API error check

            //remember who is logged in and which role they have, on every page
            HttpContext.Session.SetInt32("UserId", result.Data.UserId);
            HttpContext.Session.SetString("Role", result.Data.Role);
            HttpContext.Session.SetString("FullName", result.Data.FullName);

            //Organisers land on their dashboard, Participants on the event list
            if (result.Data.Role == "Organiser")
            {//start of Organiser landing
                return RedirectToAction("Dashboard", "Organiser");
            }//end of Organiser landing

            return RedirectToAction("Index", "Home");

        }//end of Login (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {//start of Logout

            //clearing the session also throws away the saved API login cookie
            HttpContext.Session.Clear();
            return RedirectToAction("Login");

        }//end of Logout

    }//end of class
}//end of namespace