using System.Collections.Generic;
using System.Threading.Tasks;
using FlightManager.Data.Models;

namespace FlightManager.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetAllAsync();
        Task<Reservation?> GetByIdAsync(int id);
        Task<Reservation> CreateAsync(Reservation reservation);
        Task ConfirmAsync(int id);
        Task DeleteAsync(int id);
    }
}