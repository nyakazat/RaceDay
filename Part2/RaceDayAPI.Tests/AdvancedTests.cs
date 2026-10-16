
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class AdvancedTests
    {
        // =====================================
        // CREATE TEMPORARY TEST DATABASE
        // =====================================
        private static RaceDayDbContext CreateDatabase()
        {
            var options =
                new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // =====================================
        // SIMULATE LOGGED-IN USER AND SESSION
        // =====================================
        private static void SignInController(
            ControllerBase controller,
            int userID,
            string role)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userID.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims, "TestAuthentication");

            var context = new DefaultHttpContext();

            context.User = new ClaimsPrincipal(identity);

            var session = new TestSession();
            session.SetInt32("UserID", userID);
            session.SetString("Role", role);

            context.Session = session;

            controller.ControllerContext =
                new ControllerContext
                {
                    HttpContext = context
                };
        }

        // =====================================
        // TEST 10: REGISTRATION SUCCEEDS
        // =====================================
        [Fact]
        public async Task ValidRegistration_CreatesUser()
        {
            using var db = CreateDatabase();

            db.Roles.Add(new Role
            {
                RoleID = 2,
                RoleName = "Participant"
            });

            await db.SaveChangesAsync();

            var controller = new AuthController(db);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "test@example.com",
                Password = "SecurePassword123!",
                RoleID = 2
            };

            var response = await controller.Register(request);

            var result = Assert.IsType<ObjectResult>(response);
            Assert.Equal(201, result.StatusCode);

            var user = await db.AppUsers.SingleAsync();

            Assert.Equal("test@example.com", user.Email);
            Assert.Equal(2, user.RoleID);
            Assert.NotEqual(request.Password, user.PasswordHash);

            var hasher = new PasswordHasher<AppUser>();

            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(
                    user, user.PasswordHash, request.Password));
        }

        // =====================================
        // TEST 11: INVALID ROLE REJECTED
        // =====================================
        [Fact]
        public async Task InvalidRole_RegistrationRejected()
        {
            using var db = CreateDatabase();

            var controller = new AuthController(db);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "invalid@example.com",
                Password = "SecurePassword123!",
                RoleID = 99
            };

            var response = await controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Empty(await db.AppUsers.ToListAsync());
        }

        // =====================================
        // TEST 12: DUPLICATE EMAIL REJECTED
        // =====================================
        [Fact]
        public async Task DuplicateEmail_RegistrationRejected()
        {
            using var db = CreateDatabase();

            db.Roles.Add(new Role
            {
                RoleID = 2,
                RoleName = "Participant"
            });

            db.AppUsers.Add(new AppUser
            {
                UserID = 2,
                RoleID = 2,
                FirstName = "Existing",
                LastName = "User",
                Email = "duplicate@example.com",
                PasswordHash = "ExistingHash"
            });

            await db.SaveChangesAsync();

            var controller = new AuthController(db);

            var request = new RegisterRequest
            {
                FirstName = "Another",
                LastName = "User",
                Email = "duplicate@example.com",
                Password = "SecurePassword123!",
                RoleID = 2
            };

            var response = await controller.Register(request);

            Assert.IsType<ConflictObjectResult>(response);
            Assert.Equal(1, await db.AppUsers.CountAsync());
        }

        // =====================================
        // TEST 13: INCORRECT PASSWORD REJECTED
        // =====================================
        [Fact]
        public async Task WrongPassword_LoginRejected()
        {
            using var db = CreateDatabase();

            var user = new AppUser
            {
                UserID = 4,
                RoleID = 2,
                FirstName = "Test",
                LastName = "User",
                Email = "login@example.com"
            };

            var hasher = new PasswordHasher<AppUser>();

            user.PasswordHash =
                hasher.HashPassword(user, "CorrectPassword123!");

            db.Roles.Add(new Role
            {
                RoleID = 2,
                RoleName = "Participant"
            });

            db.AppUsers.Add(user);
            await db.SaveChangesAsync();

            var controller = new AuthController(db);

            var response = await controller.Login(
                new LoginRequest
                {
                    Email = "login@example.com",
                    Password = "WrongPassword123!"
                });

            Assert.IsType<UnauthorizedObjectResult>(response);
        }

        // =====================================
        // CREATE TEST RACE DATA
        // =====================================
        private static async Task SeedRaceData(
            RaceDayDbContext db)
        {
            db.Roles.AddRange(
                new Role
                {
                    RoleID = 1,
                    RoleName = "Organiser"
                },
                new Role
                {
                    RoleID = 2,
                    RoleName = "Participant"
                });

            db.AppUsers.AddRange(
                new AppUser
                {
                    UserID = 1,
                    RoleID = 1,
                    FirstName = "Race",
                    LastName = "Organiser",
                    Email = "organiser@test.com",
                    PasswordHash = "TestHash"
                },
                new AppUser
                {
                    UserID = 4,
                    RoleID = 2,
                    FirstName = "Race",
                    LastName = "Participant",
                    Email = "participant@test.com",
                    PasswordHash = "TestHash"
                });

            db.Events.Add(new Event
            {
                EventID = 1003,
                EventName = "Test Marathon",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Cape Town",
                EventType = "Run",
                Distance = 10,
                OrganiserID = 1
            });

            db.Categories.Add(new Category
            {
                CategoryID = 3,
                CategoryName = "10KM Run"
            });

            db.EventCategories.Add(new EventCategory
            {
                EventCategoryID = 2,
                EventID = 1003,
                CategoryID = 3
            });

            await db.SaveChangesAsync();
        }

        // =====================================
        // TEST 14: PARTICIPANT CAN ENROL
        // =====================================
        [Fact]
        public async Task Participant_CanEnrolSuccessfully()
        {
            using var db = CreateDatabase();

            await SeedRaceData(db);

            var controller = new EnrolmentsController(db);

            SignInController(controller, 4, "Participant");

            var response = await controller.CreateEnrolment(
                new CreateEnrolmentRequest
                {
                    EventCategoryID = 2
                });

            var result =
                Assert.IsType<CreatedAtActionResult>(response);

            Assert.Equal(201, result.StatusCode);

            var enrolment = await db.Enrolments.SingleAsync();

            Assert.Equal(4, enrolment.UserID);
            Assert.Equal(2, enrolment.EventCategoryID);
            Assert.Equal("Active", enrolment.Status);
        }

        // =====================================
        // TEST 15: DUPLICATE ENROLMENT REJECTED
        // =====================================
        [Fact]
        public async Task DuplicateEnrolment_IsRejected()
        {
            using var db = CreateDatabase();

            await SeedRaceData(db);

            db.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 3,
                UserID = 4,
                EventCategoryID = 2,
                Status = "Active"
            });

            await db.SaveChangesAsync();

            var controller = new EnrolmentsController(db);

            SignInController(controller, 4, "Participant");

            var response = await controller.CreateEnrolment(
                new CreateEnrolmentRequest
                {
                    EventCategoryID = 2
                });

            Assert.IsType<ConflictObjectResult>(response);
            Assert.Equal(1, await db.Enrolments.CountAsync());
        }

        // =====================================
        // TEST 16: INVALID FINISH TIME REJECTED
        // =====================================
        [Fact]
        public async Task InvalidFinishTime_ResultRejected()
        {
            using var db = CreateDatabase();

            var controller = new ResultsController(db);

            SignInController(controller, 1, "Organiser");

            var response = await controller.CreateResult(
                new CreateResultRequest
                {
                    EnrolmentID = 3,
                    FinishTime = TimeSpan.Zero,
                    FinishPosition = 1,
                    Points = 100
                });

            Assert.IsType<BadRequestObjectResult>(response);
        }

        // =====================================
        // TEST 17: ENROLMENT OWNERSHIP PROTECTION
        // =====================================
        [Fact]
        public async Task OtherParticipant_CannotEditEnrolment()
        {
            using var db = CreateDatabase();

            await SeedRaceData(db);

            db.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 3,
                UserID = 4,
                EventCategoryID = 2,
                Status = "Active"
            });

            await db.SaveChangesAsync();

            var controller = new EnrolmentsController(db);

            SignInController(controller, 99, "Participant");

            var response = await controller.UpdateEnrolment(
                3,
                new UpdateEnrolmentRequest
                {
                    Status = "Cancelled"
                });

            Assert.IsType<ForbidResult>(response);

            var enrolment = await db.Enrolments.FindAsync(3);

            Assert.Equal("Active", enrolment!.Status);
        }
    }

    // =====================================
    // MOCK SESSION FOR UNIT TESTING
    // =====================================
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values
            = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;

        public string Id => "TestSession";

        public IEnumerable<string> Keys => _values.Keys;

        public void Clear()
        {
            _values.Clear();
        }

        public Task CommitAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _values.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _values[key] = value;
        }

        public bool TryGetValue(
            string key, out byte[] value)
        {
            if (_values.TryGetValue(key, out var stored))
            {
                value = stored;
                return true;
            }

            value = Array.Empty<byte>();
            return false;
        }
    }
}
