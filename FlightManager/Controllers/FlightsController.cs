using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using FlightManager.Models;
using FlightManager.Data.Models;
using System.Collections.Generic;

namespace FlightManager.Controllers
{
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        public async Task<IActionResult> Index()
        {
            var flights = await _flightService.GetAllAsync();

            var model = flights.Select(f => new FlightListViewModel
            {
                Id = f.Id,
                FromLocation = f.FromLocation,
                ToLocation = f.ToLocation,
                DepartureTime = f.DepartureTime,
                Duration = f.ArrivalTime - f.DepartureTime,
                EconomySeats = f.EconomySeats,
                BusinessSeats = f.BusinessSeats
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var passengers = new List<PassengerViewModel>();

            if (flight.Reservations != null)
            {
                passengers = flight.Reservations
                    .SelectMany(r => r.Passengers)
                    .Select(p => new PassengerViewModel
                    {
                        FirstName = p.FirstName,
                        MiddleName = p.MiddleName,
                        LastName = p.LastName,
                        EGN = p.EGN,
                        Phone = p.Phone,
                        Nationality = p.Nationality,
                        TicketType = p.TicketType
                    })
                    .ToList();
            }

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
                Passengers = passengers
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightAddViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.DepartureTime >= model.ArrivalTime)
            {
                ModelState.AddModelError(string.Empty, "Departure time must be before arrival time.");
                return View(model);
            }

            var flight = new Flight
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.DepartureTime >= model.ArrivalTime)
            {
                ModelState.AddModelError(string.Empty, "Departure time must be before arrival time.");
                return View(model);
            }

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

        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var passengers = new List<PassengerViewModel>();

            if (flight.Reservations != null)
            {
                passengers = flight.Reservations
                    .SelectMany(r => r.Passengers)
                    .Select(p => new PassengerViewModel
                    {
                        FirstName = p.FirstName,
                        MiddleName = p.MiddleName,
                        LastName = p.LastName,
                        EGN = p.EGN,
                        Phone = p.Phone,
                        Nationality = p.Nationality,
                        TicketType = p.TicketType
                    })
                    .ToList();
            }

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
                Passengers = passengers
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _flightService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}