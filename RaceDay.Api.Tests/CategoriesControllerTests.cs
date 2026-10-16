using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{//start of namespace

    public class CategoriesControllerTests
    {//start of class

        //builds a CategoriesController for a given logged-in user (pass null for nobody logged in)
        private static CategoriesController CreateController(RaceDayContext context, int? userId, string? role)
        {//start of CreateController
            var controller = new CategoriesController(context);
            TestHelper.SetSession(controller, userId, role);
            return controller;
        }//end of CreateController

        //adds an event straight into the test database
        private static Event AddEvent(RaceDayContext context, int organiserId)
        {//start of AddEvent
            var newEvent = new Event
            {//start of new event
                OrganiserId = organiserId,
                EventTypeId = 1,
                Name = "Test Event",
                Description = "An event for category tests.",
                EventDate = DateTime.Today.AddDays(10),
                Location = "Johannesburg",
                DistanceKm = 10,
                Status = "Open"
            };//end of new event

            context.Events.Add(newEvent);
            context.SaveChanges();
            return newEvent;
        }//end of AddEvent

        [Fact]
        public async Task CreateCategory_ByOwner_ReturnsCreated()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var theEvent = AddEvent(context, organiser.UserId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.CreateCategory(theEvent.EventId, new CreateCategoryRequest { CategoryName = "10km Open" });

            //201 Created and the category is linked to the event
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal(theEvent.EventId, context.Categories.Single().EventId);
        }//end of test

        [Fact]
        public async Task CreateCategory_AsParticipant_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var theEvent = AddEvent(context, organiser.UserId);
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.CreateCategory(theEvent.EventId, new CreateCategoryRequest { CategoryName = "10km Open" });

            //403 Forbidden and nothing was saved
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Categories);
        }//end of test

        [Fact]
        public async Task CreateCategory_ByDifferentOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", "Organiser");
            var otherOrganiser = TestHelper.AddUser(context, "other@test.com", "Organiser");
            var theEvent = AddEvent(context, owner.UserId);
            var controller = CreateController(context, otherOrganiser.UserId, "Organiser");

            var result = await controller.CreateCategory(theEvent.EventId, new CreateCategoryRequest { CategoryName = "10km Open" });

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Categories);
        }//end of test

        [Fact]
        public async Task CreateCategory_NotLoggedIn_ReturnsUnauthorized()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var theEvent = AddEvent(context, organiser.UserId);
            var controller = CreateController(context, null, null);

            var result = await controller.CreateCategory(theEvent.EventId, new CreateCategoryRequest { CategoryName = "10km Open" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }//end of test

        [Fact]
        public async Task CreateCategory_UnknownEvent_ReturnsNotFound()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.CreateCategory(9999, new CreateCategoryRequest { CategoryName = "10km Open" });

            Assert.IsType<NotFoundObjectResult>(result);
        }//end of test

        [Fact]
        public async Task GetCategories_AsParticipant_ReturnsOnlyThatEventsCategories()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var firstEvent = AddEvent(context, organiser.UserId);
            var secondEvent = AddEvent(context, organiser.UserId);
            context.Categories.Add(new Category { EventId = firstEvent.EventId, CategoryName = "5km Open" });
            context.Categories.Add(new Category { EventId = firstEvent.EventId, CategoryName = "5km Walk" });
            context.Categories.Add(new Category { EventId = secondEvent.EventId, CategoryName = "21km Open" });
            context.SaveChanges();
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.GetCategories(firstEvent.EventId);

            //200 OK with only the first event's two categories
            var ok = Assert.IsType<OkObjectResult>(result);
            var categories = Assert.IsType<List<CategoryResponse>>(ok.Value);
            Assert.Equal(2, categories.Count);
        }//end of test

        [Fact]
        public async Task GetCategories_UnknownEvent_ReturnsNotFound()
        {//start of test
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.GetCategories(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }//end of test

    }//end of class
}//end of namespace