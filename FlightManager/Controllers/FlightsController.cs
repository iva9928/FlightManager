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
        public async Task<IActionResult> Create(FlightAddViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var flight = new Data.Models.Flight
            {
                FromLocation = model.FromLocation,
                ToLocation = model.ToLocation,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                AircraftType = model.AircraftType,
                AircraftNumber = model.AircraftNumber,
                PilotName = model.PilotName,
                EconomySeats = model.EconomySeats,
                BusinessSeats = model.BusinessSeats
            };

            await _flightService.AddAsync(flight);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var model = new FlightEditViewModel
            {
                Id = flight.Id,
                FromLocation = flight.FromLocation,
                ToLocation = flight.ToLocation,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                AircraftType = flight.AircraftType,
                AircraftNumber = flight.AircraftNumber,
                PilotName = flight.PilotName,
                EconomySeats = flight.EconomySeats,
                BusinessSeats = flight.BusinessSeats
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var flight = await _flightService.GetByIdAsync(model.Id);
            if (flight == null) return NotFound();

            flight.FromLocation = model.FromLocation;
            flight.ToLocation = model.ToLocation;
            flight.DepartureTime = model.DepartureTime;
            flight.ArrivalTime = model.ArrivalTime;
            flight.AircraftType = model.AircraftType;
            flight.AircraftNumber = model.AircraftNumber;
            flight.PilotName = model.PilotName;
            flight.EconomySeats = model.EconomySeats;
            flight.BusinessSeats = model.BusinessSeats;

            await _flightService.UpdateAsync(flight);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _flightService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var model = new FlightDetailsViewModel
            {
                Id = flight.Id,
                FromLocation = flight.FromLocation,
                ToLocation = flight.ToLocation,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                AircraftType = flight.AircraftType,
                AircraftNumber = flight.AircraftNumber,
                PilotName = flight.PilotName,
                EconomySeats = flight.EconomySeats,
                BusinessSeats = flight.BusinessSeats,
                Passengers = flight.Reservations?.SelectMany(r => r.Passengers).Select(p => new PassengerViewModel
                {
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    EGN = p.EGN,
                    Phone = p.Phone,
                    Nationality = p.Nationality,
                    TicketType = p.TicketType
                }) ?? Enumerable.Empty<PassengerViewModel>()
            };

            return View(model);
        }
    }
}