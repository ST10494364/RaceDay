using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{//start of namespace

    public class EventsControllerTests
    {//start of class

        //builds an EventsController for a given logged-in user (pass null for nobody logged in)
        private static EventsController CreateController(RaceDayContext context, int? userId, string? role)
        {//start of CreateController
            var controller = new EventsController(context);
            TestHelper.SetSession(controller, userId, role);
            return controller;
        }//end of CreateController

        //valid request body used by the create and update tests
        private static CreateEventRequest ValidCreateRequest()
        {//start of ValidCreateRequest
            return new CreateEventRequest
            {//start of request
                Name = "Test Fun Run",
                Description = "A test event.",
                EventDate = DateTime.Today.AddDays(30),
                Location = "Johannesburg",
                DistanceKm = 5,
                EventTypeId = 1
            };//end of request
        }//end of ValidCreateRequest

        //adds an event straight into the test database
        private static Event AddEvent(RaceDayContext context, int organiserId)
        {//start of AddEvent
            var newEvent = new Event
            {//start of new event
                OrganiserId = organiserId,
                EventTypeId = 1,
                Name = "Existing Event",
                Description = "Already in the database.",
                EventDate = DateTime.Today.AddDays(10),
                Location = "Pretoria",
                DistanceKm = 10,
                Status = "Open"
            };//end of new event

            context.Events.Add(newEvent);
            context.SaveChanges();
            return newEvent;
        }//end of AddEvent

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.CreateEvent(ValidCreateRequest());

            //201 Created and the event belongs to the logged-in Organiser
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(organiser.UserId, context.Events.Single().OrganiserId);
        }//end of test

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.CreateEvent(ValidCreateRequest());

            //403 Forbidden and nothing was saved
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Events);
        }//end of test

        [Fact]
        public async Task CreateEvent_NotLoggedIn_ReturnsUnauthorized()
        {//start of test
            var context = TestHelper.CreateContext();
            var controller = CreateController(context, null, null);

            var result = await controller.CreateEvent(ValidCreateRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }//end of test

        [Fact]
        public async Task CreateEvent_InvalidEventType_ReturnsBadRequest()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var controller = CreateController(context, organiser.UserId, "Organiser");
            var request = ValidCreateRequest();
            request.EventTypeId = 999;

            var result = await controller.CreateEvent(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }//end of test

        [Fact]
        public async Task GetAllEvents_LoggedIn_ReturnsOk()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            AddEvent(context, organiser.UserId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.GetAllEvents();

            var ok = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsType<List<EventResponse>>(ok.Value);
            Assert.Single(events);
        }//end of test

        [Fact]
        public async Task GetEventById_UnknownId_ReturnsNotFound()
        {//start of test
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.GetEventById(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }//end of test

        [Fact]
        public async Task UpdateEvent_ByOwner_ReturnsOk()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var existing = AddEvent(context, organiser.UserId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.UpdateEvent(existing.EventId, new UpdateEventRequest
            {//start of request
                Name = "Renamed Event",
                Description = "Updated description.",
                EventDate = DateTime.Today.AddDays(20),
                Location = "Cape Town",
                DistanceKm = 21,
                EventTypeId = 2,
                Status = "Closed"
            });//end of request

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Renamed Event", context.Events.Single().Name);
            Assert.Equal("Closed", context.Events.Single().Status);
        }//end of test

        [Fact]
        public async Task UpdateEvent_ByDifferentOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", "Organiser");
            var otherOrganiser = TestHelper.AddUser(context, "other@test.com", "Organiser");
            var existing = AddEvent(context, owner.UserId);
            var controller = CreateController(context, otherOrganiser.UserId, "Organiser");

            var result = await controller.UpdateEvent(existing.EventId, new UpdateEventRequest
            {//start of request
                Name = "Hijacked Event",
                Description = "Should not be saved.",
                EventDate = DateTime.Today.AddDays(20),
                Location = "Nowhere",
                DistanceKm = 1,
                EventTypeId = 1,
                Status = "Open"
            });//end of request

            //403 Forbidden and the event name is unchanged
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Equal("Existing Event", context.Events.Single().Name);
        }//end of test

        [Fact]
        public async Task DeleteEvent_ByOwner_RemovesEventAndItsCategories()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var existing = AddEvent(context, organiser.UserId);
            context.Categories.Add(new Category { EventId = existing.EventId, CategoryName = "5km Open" });
            context.SaveChanges();
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.DeleteEvent(existing.EventId);

            //204 No Content and both the event and its category are gone
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Events);
            Assert.Empty(context.Categories);
        }//end of test

        [Fact]
        public async Task DeleteEvent_AsParticipant_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var existing = AddEvent(context, organiser.UserId);
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.DeleteEvent(existing.EventId);

            //403 Forbidden and the event still exists
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Single(context.Events);
        }//end of test

    }//end of class
}//end of namespace