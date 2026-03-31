using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using FlightManager.Models;
using FlightManager.Data.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FlightManager.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IFlightService _flightService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationService reservationService,
            IFlightService flightService,
            UserManager<ApplicationUser> userManager,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _flightService = flightService;
            _userManager = userManager;
            _logger = logger;
        }

        // 🔴 ADMIN + EMPLOYEE виждат ВСИЧКО
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Index()
        {
            var reservations = await _reservationService.GetAllAsync();

            var model = reservations.Select(r => new ReservationListViewModel
            {
                Id = r.Id,
                Email = r.Email,
                FlightId = r.FlightId,
                Confirmed = r.Confirmed,
                PassengersCount = r.Passengers.Count,
                Status = r.Status
            });

            return View(model);
        }

        // 🟡 USER вижда само своите
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyReservations()
        {
            var userId = _userManager.GetUserId(User);

            var reservations = (await _reservationService.GetAllAsync())
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationListViewModel
                {
                    Id = r.Id,
                    Email = r.Email,
                    FlightId = r.FlightId,
                    Confirmed = r.Confirmed,
                    PassengersCount = r.Passengers.Count
                });

            return View("Index", reservations);
        }
        // 🌍 CREATE – всички логнати
        [Authorize]
        public async Task<IActionResult> Create(int flightId)
        {
            var flight = await _flightService.GetByIdAsync(flightId);
            if (flight == null) return NotFound();

            return View(new ReservationCreateViewModel
            {
                FlightId = flightId,
                Passengers = new List<PassengerViewModel>
                {
                    new PassengerViewModel()
                }
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var reservation = new Reservation
            {
                FlightId = model.FlightId,
                Email = model.Email,
                UserId = _userManager.GetUserId(User), // 🔥 ВАЖНО
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

            await _reservationService.CreateAsync(reservation);

            TempData["Success"] = "Reservation created!";
            return RedirectToAction(nameof(MyReservations));
        }

        // 🔵 EMPLOYEE одобрява
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Confirm(int id)
        {
            await _reservationService.ConfirmAsync(id);

            TempData["Success"] = "Reservation approved!";

            return RedirectToAction(nameof(Index));
        }
        // 🟡 USER може да трие САМО своите
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);

            if (reservation == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // 🔥 защита
            if (reservation.UserId != userId)
                return Unauthorized();

            await _reservationService.DeleteAsync(id);

            return RedirectToAction(nameof(MyReservations));
        }
    }
}