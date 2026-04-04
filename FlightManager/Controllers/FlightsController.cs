using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FlightManager.Models;

namespace FlightManager.Controllers
{
    /// <summary>
    /// Контролер за управление на полети — преглед, създаване, редактиране и изтриване.
    /// </summary>
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="FlightsController"/>.
        /// </summary>
        /// <param name="flightService">Услуга за работа с полети.</param>
        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        /// <summary>
        /// Показва списък с всички полети с поддръжка на търсене и странициране.
        /// Достъпно за всички потребители, включително анонимни.
        /// </summary>
        /// <param name="search">Низ за търсене по начална или крайна дестинация.</param>
        /// <param name="page">Номер на текущата страница (по подразбиране 1).</param>
        /// <returns>Изглед със списък от полети.</returns>
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

        /// <summary>
        /// Показва формата за създаване на нов полет.
        /// Достъпно само за администратори.
        /// </summary>
        /// <returns>Изглед за създаване на полет.</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        /// <summary>
        /// Обработва POST заявка за създаване на нов полет.
        /// Достъпно само за администратори.
        /// </summary>
        /// <param name="model">Модел с данни за новия полет.</param>
        /// <returns>
        /// Пренасочване към списъка с полети при успех;
        /// изглед с грешки при невалидни данни.
        /// </returns>
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

        /// <summary>
        /// Показва формата за редактиране на съществуващ полет.
        /// Достъпно само за администратори.
        /// </summary>
        /// <param name="id">Идентификатор на полета.</param>
        /// <returns>
        /// Изглед за редактиране при намерен полет;
        /// <see cref="NotFoundResult"/> ако полетът не съществува.
        /// </returns>
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

        /// <summary>
        /// Обработва POST заявка за редактиране на съществуващ полет.
        /// Достъпно само за администратори.
        /// </summary>
        /// <param name="model">Модел с актуализирани данни за полета.</param>
        /// <returns>
        /// Пренасочване към списъка с полети при успех;
        /// изглед с грешки при невалидни данни или ненамерен полет.
        /// </returns>
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

        /// <summary>
        /// Изтрива полет по зададен идентификатор.
        /// Достъпно само за администратори.
        /// </summary>
        /// <param name="id">Идентификатор на полета за изтриване.</param>
        /// <returns>Пренасочване към списъка с полети.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _flightService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Показва детайлна информация за конкретен полет, включително списък с пасажери.
        /// Достъпно само за автентикирани потребители.
        /// </summary>
        /// <param name="id">Идентификатор на полета.</param>
        /// <returns>
        /// Изглед с детайли за полета при намерен запис;
        /// <see cref="NotFoundResult"/> ако полетът не съществува.
        /// </returns>
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