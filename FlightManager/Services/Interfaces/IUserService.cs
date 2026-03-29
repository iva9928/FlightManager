// FlightManager\Services\Interfaces\IUserService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightManager.Data.Models;

namespace FlightManager.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(string id);
    }
}