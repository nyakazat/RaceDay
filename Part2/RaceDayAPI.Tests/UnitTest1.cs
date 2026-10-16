
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using RaceDayAPI.Controllers;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class RaceDayTests
    {
        // =====================================
        // HELPER METHOD FOR VALIDATION
        // =====================================
        private static bool IsValid(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(
                model,
                context,
                results,
                validateAllProperties: true);
        }

        // =====================================
        // TEST 1: VALID EVENT
        // =====================================
        [Fact]
        public void ValidEventRequest_PassesValidation()
        {
            var request = new EventRequest
            {
                EventName = "Cape Town Marathon",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Cape Town",
                EventType = "Run",
                Distance = 10
            };

            Assert.True(IsValid(request));
        }

        // =====================================
        // TEST 2: INVALID EVENT TYPE
        // =====================================
        [Fact]
        public void InvalidEventType_FailsValidation()
        {
            var request = new EventRequest
            {
                EventName = "Test Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Cape Town",
                EventType = "Swimming",
                Distance = 10
            };

            Assert.False(IsValid(request));
        }

        // =====================================
        // TEST 3: NEGATIVE DISTANCE
        // =====================================
        [Fact]
        public void NegativeDistance_FailsValidation()
        {
            var request = new EventRequest
            {
                EventName = "Test Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Cape Town",
                EventType = "Run",
                Distance = -5
            };

            Assert.False(IsValid(request));
        }

        // =====================================
        // TEST 4: INVALID ENROLMENT CATEGORY
        // =====================================
        [Fact]
        public void InvalidEventCategoryID_FailsValidation()
        {
            var request = new CreateEnrolmentRequest
            {
                EventCategoryID = 0
            };

            Assert.False(IsValid(request));
        }

        // =====================================
        // TEST 5: INVALID FINISH POSITION
        // =====================================
        [Fact]
        public void InvalidFinishPosition_FailsValidation()
        {
            var request = new CreateResultRequest
            {
                EnrolmentID = 3,
                FinishTime = new TimeSpan(0, 52, 30),
                FinishPosition = 0,
                Points = 100
            };

            Assert.False(IsValid(request));
        }

        // =====================================
        // TEST 6: NEGATIVE POINTS
        // =====================================
        [Fact]
        public void NegativePoints_FailsValidation()
        {
            var request = new CreateResultRequest
            {
                EnrolmentID = 3,
                FinishTime = new TimeSpan(0, 52, 30),
                FinishPosition = 1,
                Points = -10
            };

            Assert.False(IsValid(request));
        }

        // =====================================
        // TEST 7: EVENT ROLE RESTRICTION
        // =====================================
        [Fact]
        public void CreateEvent_RequiresOrganiserRole()
        {
            var method = typeof(EventsController)
                .GetMethod("CreateEvent");

            Assert.NotNull(method);

            var attribute = method!
                .GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal("Organiser", attribute!.Roles);
        }

        // =====================================
        // TEST 8: ENROLMENT ROLE RESTRICTION
        // =====================================
        [Fact]
        public void CreateEnrolment_RequiresParticipantRole()
        {
            var method = typeof(EnrolmentsController)
                .GetMethod("CreateEnrolment");

            Assert.NotNull(method);

            var attribute = method!
                .GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal("Participant", attribute!.Roles);
        }

        // =====================================
        // TEST 9: RESULT ROLE RESTRICTION
        // =====================================
        [Fact]
        public void CreateResult_RequiresOrganiserRole()
        {
            var method = typeof(ResultsController)
                .GetMethod("CreateResult");

            Assert.NotNull(method);

            var attribute = method!
                .GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal("Organiser", attribute!.Roles);
        }
    }
}
