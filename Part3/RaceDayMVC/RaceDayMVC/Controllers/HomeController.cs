
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using RaceDayMVC.ViewModels;

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EventsApiService _eventsApiService;

        public HomeController(
            ILogger<HomeController> logger,
            EventsApiService eventsApiService)
        {
            _logger = logger;
            _eventsApiService = eventsApiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Events()
        {
            try
            {
                var events = await _eventsApiService.GetEventsAsync();
                return View(events);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Unable to connect to the RaceDay API. Please try again later.";
                return View(new List<EventViewModel>());
            }
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            try
            {
                var raceEvent = await _eventsApiService.GetEventAsync(id);

                if (raceEvent == null)
                    return NotFound();

                var categories = await _eventsApiService
                    .GetCategoriesForEventAsync(id);

                var viewModel = new EventDetailsViewModel
                {
                    Event = raceEvent,
                    Categories = categories
                };

                return View(viewModel);
            }
            catch (HttpRequestException)
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
            }
        }

        // PARTICIPANT ENROLMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            int eventCategoryId, int eventId)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userID.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                bool success = await _eventsApiService
                    .CreateEnrolmentAsync(eventCategoryId);

                if (success)
                {
                    TempData["Success"] =
                        "You have successfully enrolled in this event!";
                }
                else
                {
                    TempData["Error"] =
                        "Unable to enrol. You may already be enrolled in this category.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to connect to the server. Please try again.";
            }

            return RedirectToAction("EventDetails", new { id = eventId });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
