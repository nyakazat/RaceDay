
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Services
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext =>
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "HTTP context is unavailable.");

        private HttpClient Client =>
            _httpClientFactory.CreateClient("RaceDayAPI");

        private const string ApiCookiesKey = "ApiCookies";

        private void SaveApiCookies(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
                return;

            var cookiePairs = cookies
                .Select(cookie => cookie.Split(';')[0])
                .Where(cookie => !string.IsNullOrWhiteSpace(cookie));

            HttpContext.Session.SetString(
                ApiCookiesKey,
                string.Join("; ", cookiePairs));
        }

        private void AddApiCookies(HttpRequestMessage request)
        {
            string? cookies =
                HttpContext.Session.GetString(ApiCookiesKey);

            if (!string.IsNullOrEmpty(cookies))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookies);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            int roleID,
            string? phoneNumber)
        {
            var request = new
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                RoleID = roleID,
                PhoneNumber = phoneNumber
            };

            try
            {
                var response = await Client.PostAsJsonAsync(
                    "api/Auth/register", request);

                string message =
                    await response.Content.ReadAsStringAsync();

                return (
                    response.IsSuccessStatusCode,
                    message
                );
            }
            catch (HttpRequestException)
            {
                return (false, "Unable to connect to RaceDay API.");
            }
        }

        public async Task<(bool Success, string Message)> LoginAsync(
            string email,
            string password)
        {
            var request = new
            {
                Email = email,
                Password = password
            };

            try
            {
                var response = await Client.PostAsJsonAsync(
                    "api/Auth/login", request);

                string message =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, message);

                SaveApiCookies(response);

                using var document = JsonDocument.Parse(message);

                var root = document.RootElement;

                HttpContext.Session.SetInt32(
                    "UserID",
                    root.GetProperty("userID").GetInt32());

                HttpContext.Session.SetString(
                    "FirstName",
                    root.GetProperty("firstName").GetString() ?? "");

                HttpContext.Session.SetString(
                    "Email",
                    root.GetProperty("email").GetString() ?? "");

                HttpContext.Session.SetString(
                    "Role",
                    root.GetProperty("role").GetString() ?? "");

                return (true, "Login successful.");
            }
            catch (HttpRequestException)
            {
                return (false, "Unable to connect to RaceDay API.");
            }
            catch (JsonException)
            {
                return (false, "Invalid response received from API.");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get, "api/Auth/me");

                AddApiCookies(request);

                var response = await Client.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post, "api/Auth/logout");

                AddApiCookies(request);

                await Client.SendAsync(request);
            }
            finally
            {
                HttpContext.Session.Clear();
            }
        }
    }
}
