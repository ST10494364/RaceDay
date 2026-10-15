using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{//start of namespace

    public class EnrolmentsControllerTests
    {//start of class

        //builds an EnrolmentsController for a given logged-in user (pass null for nobody logged in)
        private static EnrolmentsController CreateController(RaceDayContext context, int? userId, string? role)
        {//start of CreateController
            var controller = new EnrolmentsController(context);
            TestHelper.SetSession(controller, userId, role);
            return controller;
        }//end of CreateController

        //adds an event with one category straight into the test database
        private static (Event theEvent, Category category) AddEventWithCategory(RaceDayContext context, int organiserId)
        {//start of AddEventWithCategory
            var newEvent = new Event
            {//start of new event
                OrganiserId = organiserId,
                EventTypeId = 1,
                Name = "Test Event",
                Description = "An event for enrolment tests.",
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

        //adds an enrolment straight into the test database
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

        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreated()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.Enrol(theEvent.EventId, new CreateEnrolmentRequest { CategoryId = category.CategoryId });

            //201 Created and the enrolment belongs to the logged-in Participant
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal(participant.UserId, context.Enrolments.Single().ParticipantId);
        }//end of test

        [Fact]
        public async Task Enrol_AsOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.Enrol(theEvent.EventId, new CreateEnrolmentRequest { CategoryId = category.CategoryId });

            //403 Forbidden and nothing was saved
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Enrolments);
        }//end of test

        [Fact]
        public async Task Enrol_NotLoggedIn_ReturnsUnauthorized()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, null, null);

            var result = await controller.Enrol(theEvent.EventId, new CreateEnrolmentRequest { CategoryId = category.CategoryId });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }//end of test

        [Fact]
        public async Task Enrol_Twice_ReturnsConflict()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, participant.UserId, "Participant");
            var request = new CreateEnrolmentRequest { CategoryId = category.CategoryId };

            await controller.Enrol(theEvent.EventId, request);
            var result = await controller.Enrol(theEvent.EventId, request);

            //409 Conflict and still only one enrolment
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Single(context.Enrolments);
        }//end of test

        [Fact]
        public async Task Enrol_CategoryFromDifferentEvent_ReturnsBadRequest()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (firstEvent, firstCategory) = AddEventWithCategory(context, organiser.UserId);
            var (secondEvent, secondCategory) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, participant.UserId, "Participant");

            //try to enrol in the first event using the second event's category
            var result = await controller.Enrol(firstEvent.EventId, new CreateEnrolmentRequest { CategoryId = secondCategory.CategoryId });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Enrolments);
        }//end of test

        [Fact]
        public async Task Enrol_UnknownEvent_ReturnsNotFound()
        {//start of test
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.Enrol(9999, new CreateEnrolmentRequest { CategoryId = 1 });

            Assert.IsType<NotFoundObjectResult>(result);
        }//end of test

        [Fact]
        public async Task GetEnrolments_ByOwner_ReturnsOk()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participant.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.GetEnrolments(theEvent.EventId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var enrolments = Assert.IsType<List<EnrolmentResponse>>(ok.Value);
            Assert.Single(enrolments);
        }//end of test

        [Fact]
        public async Task GetEnrolments_ByDifferentOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", "Organiser");
            var otherOrganiser = TestHelper.AddUser(context, "other@test.com", "Organiser");
            var (theEvent, category) = AddEventWithCategory(context, owner.UserId);
            var controller = CreateController(context, otherOrganiser.UserId, "Organiser");

            var result = await controller.GetEnrolments(theEvent.EventId);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }//end of test

        [Fact]
        public async Task GetEnrolments_AsParticipant_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participant = TestHelper.AddUser(context, "part@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            var controller = CreateController(context, participant.UserId, "Participant");

            var result = await controller.GetEnrolments(theEvent.EventId);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }//end of test

        [Fact]
        public async Task GetMyEnrolments_AsParticipant_ReturnsOnlyOwnEnrolments()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var participantOne = TestHelper.AddUser(context, "one@test.com", "Participant");
            var participantTwo = TestHelper.AddUser(context, "two@test.com", "Participant");
            var (theEvent, category) = AddEventWithCategory(context, organiser.UserId);
            AddEnrolment(context, participantOne.UserId, theEvent.EventId, category.CategoryId);
            AddEnrolment(context, participantTwo.UserId, theEvent.EventId, category.CategoryId);
            var controller = CreateController(context, participantOne.UserId, "Participant");

            var result = await controller.GetMyEnrolments();

            //participant one sees only their own enrolment, not participant two's
            var ok = Assert.IsType<OkObjectResult>(result);
            var enrolments = Assert.IsType<List<EnrolmentResponse>>(ok.Value);
            Assert.Single(enrolments);
            Assert.Equal(participantOne.UserId, enrolments[0].ParticipantId);
        }//end of test

        [Fact]
        public async Task GetMyEnrolments_AsOrganiser_ReturnsForbidden()
        {//start of test
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", "Organiser");
            var controller = CreateController(context, organiser.UserId, "Organiser");

            var result = await controller.GetMyEnrolments();

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }//end of test

    }//end of class
}//end of namespace