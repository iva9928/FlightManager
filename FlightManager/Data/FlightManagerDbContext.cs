using FlightManager.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Data
{
    public class FlightManagerDbContext : IdentityDbContext<ApplicationUser>
    {
        public FlightManagerDbContext(DbContextOptions<FlightManagerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Passenger> Passengers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Flight → Reservations
            builder.Entity<Flight>()
                .HasMany(f => f.Reservations)
                .WithOne(r => r.Flight)
                .HasForeignKey(r => r.FlightId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reservation → Passengers
            builder.Entity<Reservation>()
                .HasMany(r => r.Passengers)
                .WithOne(p => p.Reservation)
                .HasForeignKey(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}