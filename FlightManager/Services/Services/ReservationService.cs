using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightManager.Data;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Services.Services
{
    /// <summary>
    /// Услуга за управление на резервации — имплементация на <see cref="IReservationService"/>.
    /// Предоставя операции за създаване, потвърждаване, отмяна и изтриване на резервации.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly FlightManagerDbContext _db;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="ReservationService"/>.
        /// </summary>
        /// <param name="db">Контекст на базата данни.</param>
        public ReservationService(FlightManagerDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Връща всички резервации заедно с техните пасажери и свързани полети.
        /// </summary>
        /// <returns>Колекция от всички резервации.</returns>
        public async Task<IEnumerable<Reservation>> GetAllAsync()
            => await _db.Reservations
                .Include(r => r.Passengers)
                .Include(r => r.Flight)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Връща конкретна резервация по идентификатор, заедно с пасажерите и полета.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията.</param>
        /// <returns>
        /// Намерената резервация с включени данни;
        /// <c>null</c> ако резервацията не съществува.
        /// </returns>
        public async Task<Reservation?> GetByIdAsync(int id)
            => await _db.Reservations
                .Include(r => r.Passengers)
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(r => r.Id == id);

        /// <summary>
        /// Създава нова резервация със статус "Pending" и неподтвърдено състояние.
        /// Новите резервации винаги изчакват одобрение от служител.
        /// </summary>
        /// <param name="reservation">Обект на резервацията за създаване.</param>
        /// <returns>Създадената резервация със зададен статус.</returns>
        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            reservation.Status = "Pending";
            reservation.Confirmed = false;
            await _db.Reservations.AddAsync(reservation);
            await _db.SaveChangesAsync();
            return reservation;
        }

        /// <summary>
        /// Потвърждава резервация и намалява наличните места в съответния полет.
        /// Ако няма достатъчно места, резервацията получава статус "Rejected".
        /// Операцията се извършва само от служители.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията за потвърждаване.</param>
        public async Task ConfirmAsync(int id)
        {
            var res = await _db.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return;

            var flight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == res.FlightId);
            if (flight == null) return;

            // Преброяване на пасажерите по тип билет
            var businessCount = res.Passengers.Count(p =>
                p.TicketType.ToLower() == "business");
            var economyCount = res.Passengers.Count(p =>
                p.TicketType.ToLower() != "business");

            bool hasSeats =
                flight.BusinessSeats >= businessCount &&
                flight.EconomySeats >= economyCount;

            if (hasSeats)
            {
                // Одобряване и намаляване на свободните места
                res.Status = "Approved";
                res.Confirmed = true;
                flight.BusinessSeats -= businessCount;
                flight.EconomySeats -= economyCount;
            }
            else
            {
                // Отхвърляне поради липса на достатъчно места
                res.Status = "Rejected";
                res.Confirmed = false;
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Отменя потвърждението на резервация и връща заетите места като налични в полета.
        /// Операцията се прилага само върху вече потвърдени резервации.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията за отмяна на потвърждението.</param>
        public async Task UnconfirmAsync(int id)
        {
            var res = await _db.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return;

            // Само потвърдени резервации могат да бъдат отменени
            if (!res.Confirmed) return;

            var flight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == res.FlightId);
            if (flight == null) return;

            // Връщане на местата като налични
            var businessCount = res.Passengers.Count(p => p.TicketType.ToLower() == "business");
            var economyCount = res.Passengers.Count(p => p.TicketType.ToLower() != "business");
            flight.BusinessSeats += businessCount;
            flight.EconomySeats += economyCount;

            res.Confirmed = false;
            res.Status = "Pending";

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Изтрива резервация по идентификатор, само ако тя не е потвърдена.
        /// Потвърдените резервации не могат да бъдат изтривани.
        /// </summary>
        /// <param name="id">Идентификатор на резервацията за изтриване.</param>
        public async Task DeleteAsync(int id)
        {
            var res = await _db.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return;

            // Потвърдените резервации не се изтриват
            if (res.Confirmed) return;

            _db.Reservations.Remove(res);
            await _db.SaveChangesAsync();
        }
    }
}