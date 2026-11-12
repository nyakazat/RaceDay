
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMVC.Services;
using RaceDayMVC.ViewModels;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly EventsApiService _eventsApiService;

        public OrganiserController(
            EventsApiService eventsApiService)
        {
            _eventsApiService = eventsApiService;
        }

        private bool IsOrganiser()
        {
            int? userId =
                HttpContext.Session.GetInt32("UserID");

            string? role =
                HttpContext.Session.GetString("Role");

            return userId.HasValue && role == "Organiser";
        }

        public IActionResult Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // MANAGE EVENTS
        public async Task<IActionResult> ManageEvents()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var events =
                    await _eventsApiService.GetEventsAsync();

                int organiserId =
                    HttpContext.Session.GetInt32("UserID")!.Value;

                var myEvents = events
                    .Where(e => e.OrganiserID == organiserId)
                    .ToList();

                return View(myEvents);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Unable to load events. Please try again.";
                return View(new List<EventViewModel>());
            }
        }

        // PARTICIPANTS REGISTERED FOR ORGANISER'S EVENTS
        public async Task<IActionResult> Participants()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var enrolments =
                    await _eventsApiService.GetOrganiserEnrolmentsAsync();

                var events =
                    await _eventsApiService.GetEventsAsync();

                int organiserId =
                    HttpContext.Session.GetInt32("UserID")!.Value;

                var myEvents = events
                    .Where(e => e.OrganiserID == organiserId)
                    .ToList();

                ViewBag.Events = myEvents;

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "Unable to load participants. Please try again.";

                ViewBag.Events = new List<EventViewModel>();

                return View(new List<EnrolmentViewModel>());
            }
        }

        // ORGANISER RESULTS
        public async Task<IActionResult> Results()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var results =
                    await _eventsApiService.GetOrganiserResultsAsync();

                var events =
                    await _eventsApiService.GetEventsAsync();

                int organiserId =
                    HttpContext.Session.GetInt32("UserID")!.Value;

                var myEvents = events
                    .Where(e => e.OrganiserID == organiserId)
                    .ToList();

                ViewBag.Events = myEvents;

                return View(results);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "Unable to load results. Please try again.";

                ViewBag.Events = new List<EventViewModel>();

                return View(new List<ResultViewModel>());
            }
        }

        // CREATE EVENT - SHOW FORM
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View(new EventViewModel
            {
                EventDate = DateTime.Today.AddDays(1)
            });
        }

        // CREATE EVENT - SAVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
            EventViewModel eventModel)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(eventModel);

            try
            {
                bool success =
                    await _eventsApiService.CreateEventAsync(eventModel);

                if (success)
                {
                    TempData["Success"] = "Event created successfully!";
                    return RedirectToAction("ManageEvents");
                }

                ModelState.AddModelError(
                    "",
                    "Unable to create event. Please check your details.");

                return View(eventModel);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to connect to the API. Please try again.");

                return View(eventModel);
            }
        }

        // SHOW EDIT EVENT FORM
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var raceEvent =
                    await _eventsApiService.GetEventAsync(id);

                if (raceEvent == null ||
                    raceEvent.OrganiserID !=
                    HttpContext.Session.GetInt32("UserID"))
                {
                    return NotFound();
                }

                return View(raceEvent);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to load the event.";
                return RedirectToAction("ManageEvents");
            }
        }

        // SAVE EDITED EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            int id,
            EventViewModel eventModel)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(eventModel);

            try
            {
                bool success =
                    await _eventsApiService.UpdateEventAsync(
                        id, eventModel);

                if (success)
                {
                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction("ManageEvents");
                }

                ModelState.AddModelError(
                    "",
                    "Unable to update event. Check that you own this event.");

                return View(eventModel);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to connect to the API. Please try again.");

                return View(eventModel);
            }
        }

        // DELETE EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                bool success =
                    await _eventsApiService.DeleteEventAsync(id);

                TempData[success ? "Success" : "Error"] =
                    success
                    ? "Event deleted successfully!"
                    : "Unable to delete event. It may have registered participants or results.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to connect to the API.";
            }

            return RedirectToAction("ManageEvents");
        }
    }
}
