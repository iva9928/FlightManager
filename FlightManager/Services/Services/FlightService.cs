using System.Collections.Generic;
using System.Threading.Tasks;
using FlightManager.Data;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Services.Services
{
    /// <summary>
    /// Услуга за управление на полети — имплементация на <see cref="IFlightService"/>.
    /// Предоставя CRUD операции върху полети в базата данни.
    /// </summary>
    public class FlightService : IFlightService
    {
        private readonly FlightManagerDbContext _db;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="FlightService"/>.
        /// </summary>
        /// <param name="db">Контекст на базата данни.</param>
        public FlightService(FlightManagerDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Връща всички полети от базата данни без проследяване на промените.
        /// </summary>
        /// <returns>Колекция от всички полети.</returns>
        public async Task<IEnumerable<Flight>> GetAllAsync()
            => await _db.Flights.AsNoTracking().ToListAsync();

        /// <summary>
        /// Връща конкретен полет по идентификатор, заедно с резервациите и пасажерите му.
        /// </summary>
        /// <param name="id">Идентификатор на полета.</param>
        /// <returns>
        /// Намереният полет с включени резервации и пасажери;
        /// <c>null</c> ако полетът не съществува.
        /// </returns>
        public async Task<Flight?> GetByIdAsync(int id)
            => await _db.Flights
                .Include(f => f.Reservations)
                .ThenInclude(r => r.Passengers)
                .FirstOrDefaultAsync(f => f.Id == id);

        /// <summary>
        /// Добавя нов полет в базата данни.
        /// </summary>
        /// <param name="flight">Обект на полета за добавяне.</param>
        public async Task AddAsync(Flight flight)
        {
            await _db.Flights.AddAsync(flight);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Актуализира съществуващ полет в базата данни.
        /// </summary>
        /// <param name="flight">Обект на полета с актуализирани данни.</param>
        public async Task UpdateAsync(Flight flight)
        {
            _db.Flights.Update(flight);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Изтрива полет по идентификатор, само ако няма свързани резервации.
        /// Ако полетът има активни резервации, операцията се прекратява без грешка.
        /// </summary>
        /// <param name="id">Идентификатор на полета за изтриване.</param>
        public async Task DeleteAsync(int id)
        {
            var flight = await _db.Flights
                .Include(f => f.Reservations)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null) return;

            // Полетът не може да бъде изтрит, ако има свързани резервации
            if (flight.Reservations.Any())
                return;

            _db.Flights.Remove(flight);
            await _db.SaveChangesAsync();
        }
    }
}