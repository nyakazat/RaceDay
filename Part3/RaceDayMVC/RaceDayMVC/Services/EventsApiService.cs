
using System.Net.Http;
using System.Net.Http.Json;
using RaceDayMVC.ViewModels;

namespace RaceDayMVC.Services
{
    public class EventsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventsApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequestMessage CreateRequest(
            HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            var cookies = _httpContextAccessor.HttpContext?
                .Session.GetString("ApiCookies");

            if (!string.IsNullOrEmpty(cookies))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookies);
            }

            return request;
        }

        // GET ALL EVENTS
        public async Task<List<EventViewModel>> GetEventsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, "api/Events");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>()
                ?? new List<EventViewModel>();
        }

        // GET EVENT BY ID
        public async Task<EventViewModel?> GetEventAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, $"api/Events/{id}");

            using var response = await client.SendAsync(request);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<EventViewModel>();
        }

        // GET ALL EVENT CATEGORIES
        public async Task<List<EventCategoryViewModel>>
            GetEventCategoriesAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, "api/EventCategories");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EventCategoryViewModel>>()
                ?? new List<EventCategoryViewModel>();
        }

        // GET CATEGORIES FOR A SPECIFIC EVENT
        public async Task<List<EventCategoryViewModel>>
            GetCategoriesForEventAsync(int eventId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get,
                $"api/EventCategories/event/{eventId}");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EventCategoryViewModel>>()
                ?? new List<EventCategoryViewModel>();
        }

        // GET THE LOGGED-IN PARTICIPANT'S ENROLMENTS
        public async Task<List<EnrolmentViewModel>>
            GetMyEnrolmentsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, "api/Enrolments/my");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>()
                ?? new List<EnrolmentViewModel>();
        }

        // GET ENROLMENTS FOR THE LOGGED-IN ORGANISER'S EVENTS
        public async Task<List<EnrolmentViewModel>>
            GetOrganiserEnrolmentsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, "api/Enrolments");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>()
                ?? new List<EnrolmentViewModel>();
        }

        // GET THE LOGGED-IN PARTICIPANT'S RESULTS
        public async Task<List<ResultViewModel>>
            GetMyResultsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, "api/Results/my");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<ResultViewModel>>()
                ?? new List<ResultViewModel>();
        }

        // GET RESULTS FOR THE LOGGED-IN ORGANISER
        public async Task<List<ResultViewModel>>
            GetOrganiserResultsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Get, "api/Results");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<ResultViewModel>>()
                ?? new List<ResultViewModel>();
        }

        // CREATE A RACE RESULT
        public async Task<bool> CreateResultAsync(
            int enrolmentId,
            TimeSpan finishTime,
            int finishPosition,
            int points)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Post, "api/Results");

            request.Content = JsonContent.Create(new
            {
                enrolmentID = enrolmentId,
                finishTime = finishTime,
                finishPosition = finishPosition,
                points = points
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // UPDATE A RACE RESULT
        public async Task<bool> UpdateResultAsync(
            int id,
            TimeSpan finishTime,
            int finishPosition,
            int points)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Put, $"api/Results/{id}");

            request.Content = JsonContent.Create(new
            {
                finishTime = finishTime,
                finishPosition = finishPosition,
                points = points
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // DELETE A RACE RESULT
        public async Task<bool> DeleteResultAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Delete, $"api/Results/{id}");

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // CREATE A PARTICIPANT ENROLMENT
        public async Task<bool> CreateEnrolmentAsync(
            int eventCategoryId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Post, "api/Enrolments");

            request.Content = JsonContent.Create(new
            {
                eventCategoryID = eventCategoryId
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // UPDATE ENROLMENT STATUS
        public async Task<bool> UpdateEnrolmentStatusAsync(
            int id, string status)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Put, $"api/Enrolments/{id}");

            request.Content = JsonContent.Create(new
            {
                status = status
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // DELETE AN ENROLMENT
        public async Task<bool> DeleteEnrolmentAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Delete, $"api/Enrolments/{id}");

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // CREATE EVENT
        public async Task<bool> CreateEventAsync(
            EventViewModel eventModel)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Post, "api/Events");

            request.Content = JsonContent.Create(eventModel);

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // UPDATE EVENT
        public async Task<bool> UpdateEventAsync(
            int id, EventViewModel eventModel)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Put, $"api/Events/{id}");

            request.Content = JsonContent.Create(eventModel);

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // DELETE EVENT
        public async Task<bool> DeleteEventAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = CreateRequest(
                HttpMethod.Delete, $"api/Events/{id}");

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
