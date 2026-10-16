using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{//start of namespace

    public class ResultsControllerTests
    {//start of class

        //builds a ResultsController for a given logged-in user (pass null for nobody logged in)
        private static ResultsController CreateController(RaceDayContext context, int? userId, string? role)
        {//start of CreateController
            var controller = new ResultsController(context);
            TestHelper.SetSession(controller, userId, role);
            return controller;
        }//end of CreateController

        //adds an event with one category into the test database
        private static (Event theEvent, Category category) AddEventWithCategory(RaceDayContext context, int organiserId)
        {//start of AddEventWithCategory
            var newEvent = new Event
            {//start of new event
                OrganiserId = organiserId,
                EventTypeId = 1,
                Name = "Test Event",
                Description = "An event for result tests.",
                EventDate = DateTime.Today.AddDays(10),
                Location = "Johannesburg",
                DistanceKm = 10,
                Status = "Open"
            };//end of new event
            context.Events.Add(newEvent);
            context.SaveChanges();

            var category = new Category { EventId = newEvent.EventId, CategoryName = "10km Open" };
            context.Categories.Add(category);
            context.SaveChanges();

            return (newEvent, category);
        }//end of AddEventWithCategory

        //enrols a Participant in an event
        private static void AddEnrolment(RaceDayContext context, int participantId, int eventId, int categoryId)
        {//start of AddEnrolment
            context.Enrolments.Add(new Enrolment
            {//start of new enrolment
                ParticipantId = participantId,
                EventId = eventId,
                CategoryId = categoryId
            });//end of new enrolment
            context.SaveChanges();
        }//end of AddEnrolment

        //builds a valid result request for a Participant
        private static CreateResultRequest ValidRequest(int participantId)
        {//start of ValidRequest
            return new CreateResultRequest
            {//start of request
                ParticipantId = participantId,
                FinishTime = new TimeSpan(0, 45, 30),
                FinishingPosition = 1
            };//end of request
        }//end of ValidRequest

        [Fact]
        public async Task CreateResult_ByOwnerForEnrolledParticipant_ReturnsCreated()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));

            //201 Created and the result is saved for that Participant
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal(participant.UserId, context.Results.Single().ParticipantId);
        }//end of test

        [Fact]
        public async Task CreateResult_AsParticipant_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));

            //403 Forbidden and nothing was saved
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Results);
        }//end of test

        [Fact]
        public async Task CreateResult_ByDifferentOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", "Organiser");
            var otherOrganiser = TestHelper.AddUser(context, "other@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, owner.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, otherOrganiser.UserId, "Organiser");

            var result = await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Results);
        }//end of test

        [Fact]
        public async Task CreateResult_NotLoggedIn_ReturnsUnauthorized()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, null, null);

            var result = await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));

            Assert.IsType<UnauthorizedObjectResult>(result);
        }//end of test

        [Fact]
        public async Task CreateResult_ParticipantNotEnrolled_ReturnsBadRequest()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            //the Participant never enrolled in this event
            var result = await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Results);
        }//end of test

        [Fact]
        public async Task CreateResult_DuplicateForSameParticipant_ReturnsBadRequest()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));
            var result = await controller.CreateResult(theEvent.EventId, ValidRequest(participant.UserId));

            //400 Bad Request and still only one result
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Single(context.Results);
        }//end of test

        [Fact]
        public async Task CreateResult_ZeroFinishTime_ReturnsBadRequest()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, organiser.UserId, "Organiser");
            var request = ValidRequest(participant.UserId);
            request.FinishTime = TimeSpan.Zero;

            var result = await controller.CreateResult(theEvent.EventId, request);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Results);
        }//end of test

        [Fact]
        public async Task CreateResult_UnknownEvent_ReturnsNotFound()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.CreateResult(9999, ValidRequest(1));

            Assert.IsType<NotFoundObjectResult>(result);
        }//end of test

        [Fact]
        public async Task GetMyResults_AsParticipant_ReturnsOnlyOwnResults()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participantOne = TestHelper.AddUser(context, "one@test.com", "Participant");
            var participantTwo = TestHelper.AddUser(context, "two@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participantOne.UserId, theEvent.EventId, category.CategoryId);
            AddEnrolment(context, participantTwo.UserId, theEvent.EventId, category.CategoryId);
            context.Results.Add(new Result { ParticipantId = participantOne.UserId, EventId = theEvent.EventId, FinishTime = new TimeSpan(0, 40, 0), FinishingPosition = 1 });
            context.Results.Add(new Result { ParticipantId = participantTwo.UserId, EventId = theEvent.EventId, FinishTime = new TimeSpan(0, 50, 0), FinishingPosition = 2 });
            context.SaveChanges();
            var controller = CreateController(context, participantOne.UserId, "Participant");

            var result = await controller.GetMyResults();

            //participant one sees only their own result, not participant two's
            var ok = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsType<List<ResultResponse>>(ok.Value);
            Assert.Single(results);
            Assert.Equal(participantOne.UserId, results[0].ParticipantId);
        }//end of test

        [Fact]
        public async Task GetMyResults_AsOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.GetMyResults();

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }//end of test

    }//end of class
}//end of namespace