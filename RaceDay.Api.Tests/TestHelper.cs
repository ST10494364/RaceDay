using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.Diagnostics.CodeAnalysis;

namespace RaceDay.Api.Tests
{//start of namespace

    //a simple in-memory session so controllers can read UserId and Role during tests
    public class FakeSession : ISession
    {//start of FakeSession class

        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => "test-session";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _store.TryGetValue(key, out value);

    }//end of FakeSession class

    //shared helper methods used by all the tests
    public static class TestHelper
    {//start of TestHelper class

        //creates a brand new empty in-memory database (seeded with the three event types) for each test
        public static RaceDayContext CreateContext()
        {//start of CreateContext
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayContext(options);
            context.Database.EnsureCreated();
            return context;
        }//end of CreateContext

        //gives a controller a fake session, as if a user were logged in (pass null for nobody logged in)
        public static void SetSession(ControllerBase controller, int? userId, string? role)
        {//start of SetSession
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            if (userId != null)
            {//start of user ID check
                httpContext.Session.SetInt32("UserId", userId.Value);
            }//end of user ID check

            if (role != null)
            {//start of role check
                httpContext.Session.SetString("Role", role);
            }//end of role check

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }//end of SetSession

        //adds a user straight into the test database and returns it
        public static User AddUser(RaceDayContext context, string email, string role)
        {//start of AddUser
            var user = new User
            {//start of new user
                FullName = "Test " + role,
                Email = email,
                PasswordHash = "not-a-real-hash",
                Role = role
            };//end of new user

            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }//end of AddUser

    }//end of TestHelper class
}//end of namespace