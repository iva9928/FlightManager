// FlightManager\Services\Services\ReservationService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightManager.Data;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Services.Services
{
    public class ReservationService : IReservationService
    {
        private readonly FlightManagerDbContext _db;

        public ReservationService(FlightManagerDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync()
            => await _db.Reservations.Include(r => r.Passengers).Include(r => r.Flight).AsNoTracking().ToListAsync();

        public async Task<Reservation?> GetByIdAsync(int id)
            => await _db.Reservations.Include(r => r.Passengers).Include(r => r.Flight).FirstOrDefaultAsync(r => r.Id == id);

        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            // Load flight for availability check
            var flight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == reservation.FlightId);

            // Normalize ticket type checks (case-insensitive)
            var businessCount = reservation.Passengers.Count(p => string.Equals(p.TicketType, "business", System.StringComparison.OrdinalIgnoreCase));
            var economyCount = reservation.Passengers.Count(p => !string.Equals(p.TicketType, "business", System.StringComparison.OrdinalIgnoreCase));

            if (flight != null && flight.BusinessSeats >= businessCount && flight.EconomySeats >= economyCount)
            {
                // Enough seats -> confirm reservation and decrement seats
                reservation.Confirmed = true;

                flight.BusinessSeats -= businessCount;
                flight.EconomySeats -= economyCount;

                // Save flight changes and reservation
                await _db.Reservations.AddAsync(reservation);
                await _db.SaveChangesAsync();
            }
            else
            {
                // Not enough seats -> save as unconfirmed so admin can inspect / confirm later
                reservation.Confirmed = false;
                await _db.Reservations.AddAsync(reservation);
                await _db.SaveChangesAsync();
            }

            return reservation;
        }

        public async Task ConfirmAsync(int id)
        {
            var res = await _db.Reservations.Include(r => r.Passengers).FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return;
            if (res.Confirmed) return; // already confirmed

            var flight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == res.FlightId);
            if (flight == null) return;

            var businessCount = res.Passengers.Count(p => string.Equals(p.TicketType, "business", System.StringComparison.OrdinalIgnoreCase));
            var economyCount = res.Passengers.Count(p => !string.Equals(p.TicketType, "business", System.StringComparison.OrdinalIgnoreCase));

            if (flight.BusinessSeats >= businessCount && flight.EconomySeats >= economyCount)
            {
                flight.BusinessSeats -= businessCount;
                flight.EconomySeats -= economyCount;

                res.Confirmed = true;
                await _db.SaveChangesAsync();
            }
            // if not enough seats then do nothing (stay unconfirmed)
        }

        public async Task DeleteAsync(int id)
        {
            var res = await _db.Reservations.Include(r => r.Passengers).FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return;

            // Do not delete confirmed reservations
            if (res.Confirmed) return;

            _db.Reservations.Remove(res);
            await _db.SaveChangesAsync();
        }
    }
}