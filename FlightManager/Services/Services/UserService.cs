// FlightManager\Services\Services\UserService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlightManager.Services.Services
{
    /// <summary>
    /// Услуга за управление на потребители — имплементация на <see cref="IUserService"/>.
    /// Предоставя операции за извличане на потребителски данни чрез Identity системата.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="UserService"/>.
        /// </summary>
        /// <param name="userManager">Услуга за управление на потребителски акаунти.</param>
        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Връща всички регистрирани потребители без проследяване на промените.
        /// </summary>
        /// <returns>Колекция от всички потребители в системата.</returns>
        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
            => await _userManager.Users.AsNoTracking().ToListAsync();

        /// <summary>
        /// Връща конкретен потребител по уникален идентификатор.
        /// </summary>
        /// <param name="id">Уникален идентификатор на потребителя.</param>
        /// <returns>
        /// Намереният потребител;
        /// <c>null</c> ако потребителят не съществува.
        /// </returns>
        public async Task<ApplicationUser?> GetByIdAsync(string id)
            => await _userManager.FindByIdAsync(id);
    }
}