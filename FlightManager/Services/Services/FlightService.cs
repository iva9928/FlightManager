using System.Collections.Generic;
using System.Threading.Tasks;
using FlightManager.Data;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Services.Services
{
    public class FlightService : IFlightService
    {
        private readonly FlightManagerDbContext _db;

        public FlightService(FlightManagerDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Flight>> GetAllAsync()
            => await _db.Flights.AsNoTracking().ToListAsync();

        public async Task<Flight?> GetByIdAsync(int id)
            => await _db.Flights
                .Include(f => f.Reservations)
                .ThenInclude(r => r.Passengers)
                .FirstOrDefaultAsync(f => f.Id == id);

        public async Task AddAsync(Flight flight)
        {
            await _db.Flights.AddAsync(flight);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Flight flight)
        {
            _db.Flights.Update(flight);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var flight = await _db.Flights
                .Include(f => f.Reservations)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null) return;

            if (flight.Reservations.Any())
                return;

            _db.Flights.Remove(flight);
            await _db.SaveChangesAsync();
        }
    }
}