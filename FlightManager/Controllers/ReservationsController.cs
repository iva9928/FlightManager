using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using FlightManager.Models;
using FlightManager.Data.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FlightManager.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IFlightService _flightService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationService reservationService,
            IFlightService flightService,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _flightService = flightService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var reservations = await _reservationService.GetAllAsync();

            var model = reservations.Select(r => new ReservationListViewModel
            {
                Id = r.Id,
                Email = r.Email,
                FlightId = r.FlightId,
                Confirmed = r.Confirmed,
                PassengersCount = r.Passengers?.Count ?? 0
            });

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            var model = new ReservationCreateViewModel
            {
                FlightId = reservation.FlightId,
                Email = reservation.Email,
                Passengers = reservation.Passengers?.Select(p => new PassengerViewModel
                {
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    EGN = p.EGN,
                    Phone = p.Phone,
                    Nationality = p.Nationality,
                    TicketType = p.TicketType
                }).ToList() ?? new List<PassengerViewModel>()
            };

            ViewBag.Confirmed = reservation.Confirmed;
            ViewBag.ReservationId = reservation.Id;

            return View(model);
        }

        public async Task<IActionResult> Create(int flightId)
        {
            var flight = await _flightService.GetByIdAsync(flightId);
            if (flight == null) return NotFound();

            var model = new ReservationCreateViewModel { FlightId = flightId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reservation = new Reservation
            {
                FlightId = model.FlightId,
                Email = model.Email,
                Confirmed = false,
                Passengers = model.Passengers.Select(p => new Passenger
                {
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    EGN = p.EGN,
                    Phone = p.Phone,
                    Nationality = p.Nationality,
                    TicketType = p.TicketType
                }).ToList()
            };

            var created = await _reservationService.CreateAsync(reservation);

            if (created.Confirmed)
            {
                // send confirmation (stub: log)
                _logger.LogInformation("Reservation {ReservationId} confirmed and email sent to {Email}", created.Id, created.Email);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            await _reservationService.ConfirmAsync(id);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _reservationService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
