using FlightManager.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Data
{
    /// <summary>
    /// Контекст на базата данни за приложението FlightManager.
    /// Наследява <see cref="IdentityDbContext{TUser}"/> за поддръжка на Identity система.
    /// </summary>
    public class FlightManagerDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Инициализира нов екземпляр на <see cref="FlightManagerDbContext"/>.
        /// </summary>
        /// <param name="options">Настройки за конфигурация на контекста.</param>
        public FlightManagerDbContext(DbContextOptions<FlightManagerDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Таблица с полети в базата данни.
        /// </summary>
        public DbSet<Flight> Flights { get; set; }

        /// <summary>
        /// Таблица с резервации в базата данни.
        /// </summary>
        public DbSet<Reservation> Reservations { get; set; }

        /// <summary>
        /// Таблица с пасажери в базата данни.
        /// </summary>
        public DbSet<Passenger> Passengers { get; set; }

        /// <summary>
        /// Конфигурира релациите и ограниченията между моделите при изграждане на схемата.
        /// </summary>
        /// <param name="builder">Строител на модела, използван за конфигурация на Entity Framework.</param>
        /// <remarks>
        /// Дефинирани релации:
        /// <list type="bullet">
        /// <item><description>Полет → Резервации: един към много, с каскадно изтриване.</description></item>
        /// <item><description>Резервация → Пасажери: един към много, с каскадно изтриване.</description></item>
        /// </list>
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Полет → Резервации: един полет има много резервации
            builder.Entity<Flight>()
                .HasMany(f => f.Reservations)
                .WithOne(r => r.Flight)
                .HasForeignKey(r => r.FlightId)
                .OnDelete(DeleteBehavior.Cascade);

            // Резервация → Пасажери: една резервация има много пасажери
            builder.Entity<Reservation>()
                .HasMany(r => r.Passengers)
                .WithOne(p => p.Reservation)
                .HasForeignKey(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}