// FlightManager\Services\Services\UserService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
            => await _userManager.Users.AsNoTracking().ToListAsync();

        public async Task<ApplicationUser?> GetByIdAsync(string id)
            => await _userManager.FindByIdAsync(id);
    }
}