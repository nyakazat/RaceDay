
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;
using RaceDayMVC.ViewModels;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly EventsApiService _eventsApiService;

        public ParticipantController(
            EventsApiService eventsApiService)
        {
            _eventsApiService = eventsApiService;
        }

        public async Task<IActionResult> Index()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userID.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                var events = await _eventsApiService.GetEventsAsync();

                var enrolments = await _eventsApiService
                    .GetMyEnrolmentsAsync();

                var results = await _eventsApiService
                    .GetMyResultsAsync();

                ViewBag.MyEnrolments = enrolments;
                ViewBag.EnrolmentCount = enrolments.Count;

                ViewBag.MyResults = results;
                ViewBag.ResultCount = results.Count;

                return View(events);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "Unable to load dashboard information. Please try again later.";

                ViewBag.MyEnrolments =
                    new List<EnrolmentViewModel>();

                ViewBag.EnrolmentCount = 0;

                ViewBag.MyResults =
                    new List<ResultViewModel>();

                ViewBag.ResultCount = 0;

                return View(new List<EventViewModel>());
            }
        }
    }
}
