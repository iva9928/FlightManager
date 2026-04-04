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
    /// <summary>
    /// Контролер за управление на резервации — създаване, преглед, потвърждаване и изтриване.
    /// </summary>
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IFlightService _flightService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReservationsController> _logger;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="ReservationsController"/>.
        /// </summary>
        /// <param name="reservationService">Услуга за работа с резервации.</param>
        /// <param name="flightService">Услуга за работа с полети.</param>
        /// <param name="userManager">Услуга за управление на потребители.</param>
        /// <param name="logger">Услуга за логване.</param>
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

        /// <summary>
        /// Показва списък с всички резервации.
        /// Достъпно само за администратори и служители.
        /// </summary>
        /// <returns>Изглед със списък от всички резервации.</returns>
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

        /// <summary>
        /// Показва само резервациите на текущо влезлия потребител.
        /// Достъпно само за потребители с роля "User".
        /// </summary>
        /// <returns>Изглед със списък от резервации на текущия потребител.</returns>
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

        /// <summary>
        /// Показва формата за създаване на нова резервация за конкретен полет.
        /// Достъпно само за автентикирани потребители.
        /// </summary>
        /// <param name="flightId">Идентификатор на полета, за който се прави резервация.</param>
        /// <returns>
        /// Изглед за създаване на резервация;
        /// <see cref="NotFoundResult"/> ако полетът не съществува.
        /// </returns>
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

        /// <summary>
        /// Обработва POST заявка за създаване на нова резервация.
        /// Проверява наличността на места преди записа.
        /// Достъпно само за автентикирани потребители.
        /// </summary>
        /// <param name="model">Модел с данни за резервацията и пасажерите.</param>
        /// <returns>
        /// Пренасочване към резервациите на потребителя при успех;
        /// изглед с грешки при невалидни данни или липса на свободни места.
        /// </returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var flight = await _flightService.GetByIdAsync(model.FlightId);
            if (flight == null)
            {
                ModelState.AddModelError(string.Empty, "Избраният полет не беше намерен.");
                return View(model);
            }

            // Преброяване на заявените места по тип билет
            var requestedBusiness = model.Passengers?.Count(p => string.Equals(p.TicketType, "Business", System.StringComparison.OrdinalIgnoreCase)) ?? 0;
            var requestedTotal = model.Passengers?.Count ?? 0;
            var requestedEconomy = requestedTotal - requestedBusiness;

            // Проверка за наличност на места
            if (flight.BusinessSeats < requestedBusiness || flight.EconomySeats < requestedEconomy)
            {
                ModelState.AddModelError(string.Empty, $"Няма достатъчно свободни места. Налични: {flight.EconomySeats} икономична класа, {flight.BusinessSeats} бизнес класа.");
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

            // Създаване на резервацията — услугата потвърждава и намалява броя на местата
            await _reservationService.CreateAsync(reservation);

            TempData["Success"] = "Резервацията е създадена успешно!";
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(MyReservations));

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Потвърждава резервация по зададен идентификатор.
        /// Достъпно само за служители.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията за потвърждаване.</param>
        /// <returns>Пренасочване към списъка с резервации.</returns>
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            await _reservationService.ConfirmAsync(id);
            TempData["Success"] = "Резервацията е потвърдена!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Отменя потвърждението на резервация и връща местата като налични.
        /// Достъпно само за служители.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията.</param>
        /// <returns>Пренасочване към списъка с резервации.</returns>
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unconfirm(int id)
        {
            await _reservationService.UnconfirmAsync(id);
            TempData["Success"] = "Потвърждението е отменено и местата са върнати.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Показва детайлна информация за конкретна резервация.
        /// Администраторите и служителите виждат всяка резервация;
        /// обикновените потребители могат да виждат само своите.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията.</param>
        /// <returns>
        /// Изглед с детайли за резервацията;
        /// <see cref="NotFoundResult"/> ако резервацията не съществува;
        /// <see cref="UnauthorizedResult"/> ако потребителят няма право на достъп.
        /// </returns>
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
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

        /// <summary>
        /// Изтрива резервация, принадлежаща на текущия потребител.
        /// Потребителят може да изтрива само собствените си резервации.
        /// Достъпно само за потребители с роля "User".
        /// </summary>
        /// <param name="id">Идентификатор на резервацията за изтриване.</param>
        /// <returns>
        /// Пренасочване към резервациите на потребителя при успех;
        /// <see cref="NotFoundResult"/> ако резервацията не съществува;
        /// <see cref="UnauthorizedResult"/> ако потребителят не е собственик.
        /// </returns>
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);

            if (reservation == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // Проверка дали текущият потребител е собственик на резервацията
            if (reservation.UserId != userId)
                return Unauthorized();

            await _reservationService.DeleteAsync(id);

            return RedirectToAction(nameof(MyReservations));
        }
    }
}