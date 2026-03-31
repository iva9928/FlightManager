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
            => await _db.Reservations
                .Include(r => r.Passengers)
                .Include(r => r.Flight)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Reservation?> GetByIdAsync(int id)
            => await _db.Reservations
                .Include(r => r.Passengers)
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(r => r.Id == id);

        // 🔥 USER създава → винаги Pending
        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            reservation.Status = "Pending";
            reservation.Confirmed = false;

            await _db.Reservations.AddAsync(reservation);
            await _db.SaveChangesAsync();

            return reservation;
        }

        // 🔥 EMPLOYEE одобрява
        public async Task ConfirmAsync(int id)
        {
            var res = await _db.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return;

            var flight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == res.FlightId);
            if (flight == null) return;

            var businessCount = res.Passengers.Count(p =>
                p.TicketType.ToLower() == "business");

            var economyCount = res.Passengers.Count(p =>
                p.TicketType.ToLower() != "business");

            bool hasSeats =
                flight.BusinessSeats >= businessCount &&
                flight.EconomySeats >= economyCount;

            if (hasSeats)
            {
                res.Status = "Approved";
                res.Confirmed = true;

                flight.BusinessSeats -= businessCount;
                flight.EconomySeats -= economyCount;
            }
            else
            {
                res.Status = "Rejected";
                res.Confirmed = false;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var res = await _db.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return;

            if (res.Confirmed) return;

            _db.Reservations.Remove(res);
            await _db.SaveChangesAsync();
        }
    }
}