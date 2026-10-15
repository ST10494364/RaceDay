using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests
{//start of namespace

    public class AuthControllerTests
    {//start of class

        //builds an AuthController with a fresh database and an empty session
        private static (AuthController controller, RaceDayContext context) CreateController()
        {//start of CreateController
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context);
            TestHelper.SetSession(controller, null, null);
            return (controller, context);
        }//end of CreateController

        [Fact]
        public async Task Register_ValidRequest_ReturnsCreatedAndHashesPassword()
        {//start of test
            var (controller, context) = CreateController();
            var request = new RegisterRequest
            {//start of request
                FullName = "New Organiser",
                Email = "new@test.com",
                Password = "Password123",
                Role = "Organiser"
            };//end of request

            var result = await controller.Register(request);

            //201 Created
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);

            //the password is stored as a hash, never as plain text
            var saved = context.Users.Single(u => u.Email == "new@test.com");
            Assert.NotEqual("Password123", saved.PasswordHash);
            Assert.Equal("Organiser", saved.Role);
        }//end of test

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {//start of test
            var (controller, context) = CreateController();
            var request = new RegisterRequest
            {//start of request
                FullName = "First User",
                Email = "duplicate@test.com",
                Password = "Password123",
                Role = "Participant"
            };//end of request

            await controller.Register(request);
            var result = await controller.Register(request);

            //409 Conflict on the second attempt
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(1, context.Users.Count(u => u.Email == "duplicate@test.com"));
        }//end of test

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkAndStartsSession()
        {//start of test
            var (controller, context) = CreateController();
            await controller.Register(new RegisterRequest
            {//start of request
                FullName = "Login User",
                Email = "login@test.com",
                Password = "Password123",
                Role = "Participant"
            });//end of request

            var result = await controller.Login(new LoginRequest { Email = "login@test.com", Password = "Password123" });

            //200 OK and the session now holds the user ID and role
            Assert.IsType<OkObjectResult>(result);
            var user = context.Users.Single(u => u.Email == "login@test.com");
            Assert.Equal((int?)user.UserId, controller.HttpContext.Session.GetInt32("UserId"));
            Assert.Equal("Participant", controller.HttpContext.Session.GetString("Role"));
        }//end of test

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {//start of test
            var (controller, context) = CreateController();
            await controller.Register(new RegisterRequest
            {//start of request
                FullName = "Login User",
                Email = "wrongpass@test.com",
                Password = "Password123",
                Role = "Participant"
            });//end of request

            var result = await controller.Login(new LoginRequest { Email = "wrongpass@test.com", Password = "WrongPassword" });

            //401 Unauthorized and no session is started
            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Null(controller.HttpContext.Session.GetInt32("UserId"));
        }//end of test

        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {//start of test
            var (controller, context) = CreateController();

            var result = await controller.Login(new LoginRequest { Email = "nobody@test.com", Password = "Password123" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }//end of test

    }//end of class
}//end of namespace