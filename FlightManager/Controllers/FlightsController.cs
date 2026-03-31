using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FlightManager.Models;

namespace FlightManager.Controllers
{
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;

            var flights = await _flightService.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                flights = flights.Where(f =>
                    f.FromLocation.Contains(search) ||
                    f.ToLocation.Contains(search));
            }

            var pagedFlights = flights
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = pagedFlights.Select(f => new FlightListViewModel
            {
                Id = f.Id,
                FromLocation = f.FromLocation,
                ToLocation = f.ToLocation,
                DepartureTime = f.DepartureTime,
                Duration = f.ArrivalTime - f.DepartureTime
            });

            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Data.Models.Flight flight)
        {
            if (!ModelState.IsValid) return View(flight);

            await _flightService.AddAsync(flight);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Data.Models.Flight flight)
        {
            if (!ModelState.IsValid) return View(flight);

            await _flightService.UpdateAsync(flight);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _flightService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }
    }
}