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
        // 🌍 CREATE – всеки (анонимен достъп)
        // CREATE – authenticated users (Users, Employees, Admins)
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
            // Load flight to validate availability before creating reservation
            var flight = await _flightService.GetByIdAsync(model.FlightId);
            if (flight == null)
            {
                ModelState.AddModelError(string.Empty, "Selected flight was not found.");
                return View(model);
            }

            // Count requested seats by ticket type
            var requestedBusiness = model.Passengers?.Count(p => string.Equals(p.TicketType, "Business", System.StringComparison.OrdinalIgnoreCase)) ?? 0;
            var requestedTotal = model.Passengers?.Count ?? 0;
            var requestedEconomy = requestedTotal - requestedBusiness;

            // Check availability
            if (flight.BusinessSeats < requestedBusiness || flight.EconomySeats < requestedEconomy)
            {
                ModelState.AddModelError(string.Empty, $"Not enough available seats. Available: {flight.EconomySeats} economy, {flight.BusinessSeats} business.");
                return View(model);
            }

            var reservation = new Reservation
            {
                FlightId = model.FlightId,
                Email = model.Email,
                UserId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null,
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

            // Create reservation (service will confirm and decrement seats)
            await _reservationService.CreateAsync(reservation);

            TempData["Success"] = "Reservation created!";
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(MyReservations));

            return RedirectToAction(nameof(Index));
        }

        // 🔵 EMPLOYEE одобрява (POST)
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            await _reservationService.ConfirmAsync(id);

            TempData["Success"] = "Reservation approved!";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unconfirm(int id)
        {
            await _reservationService.UnconfirmAsync(id);

            TempData["Success"] = "Reservation unconfirmed and seats returned.";
            return RedirectToAction(nameof(Index));
        }

        // Details - allow Employee/Admin to view any, Users to view their own
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                // only allow owner to view
                if (reservation.UserId != currentUserId) return Unauthorized();
            }

            var model = new ReservationCreateViewModel
            {
                FlightId = reservation.FlightId,
                Email = reservation.Email,
                Passengers = reservation.Passengers.Select(p => new PassengerViewModel
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

            ViewBag.Confirmed = reservation.Confirmed;
            ViewBag.ReservationId = reservation.Id;

            return View(model);
        }
        // 🟡 USER може да трие САМО своите
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
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