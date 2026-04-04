using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FlightManager.Data.Models;
using FlightManager.Models;

namespace FlightManager.Controllers
{
    /// <summary>
    /// Контролер за управление на потребители — достъпен само за администратори.
    /// Предоставя функции за преглед, търсене и промяна на роли на потребители.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="UsersController"/>.
        /// </summary>
        /// <param name="userService">Услуга за работа с потребители.</param>
        /// <param name="userManager">Услуга за управление на потребителски акаунти.</param>
        /// <param name="roleManager">Услуга за управление на роли.</param>
        public UsersController(
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Показва списък с всички потребители с поддръжка на търсене и странициране.
        /// </summary>
        /// <param name="search">Низ за търсене по имейл, потребителско име, име или фамилия.</param>
        /// <param name="page">Номер на текущата страница (по подразбиране 1).</param>
        /// <param name="pageSize">Брой записи на страница (по подразбиране 10).</param>
        /// <returns>Административен изглед със списък от потребители и техните роли.</returns>
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Email.Contains(search) ||
                    u.UserName.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));
            }

            var total = users.Count();
            var model = new List<UserListViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new UserListViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = roles
                });
            }

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Search = search;

            return View("~/Views/Admin/UsersIndex.cshtml", model);
        }

        /// <summary>
        /// Показва детайлна информация за конкретен потребител.
        /// </summary>
        /// <param name="id">Идентификатор на потребителя.</param>
        /// <returns>
        /// Изглед с детайли за потребителя;
        /// <see cref="NotFoundResult"/> ако потребителят не съществува.
        /// </returns>
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        /// <summary>
        /// Обработва POST заявка за промяна на ролята на потребител.
        /// Премахва всички текущи роли и присвоява новата, ако тя съществува.
        /// </summary>
        /// <param name="id">Идентификатор на потребителя.</param>
        /// <param name="role">Името на новата роля за присвояване.</param>
        /// <returns>
        /// Пренасочване към списъка с потребители при успех;
        /// <see cref="NotFoundResult"/> ако потребителят не съществува.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Премахване на всички текущи роли
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Присвояване на новата роля, ако е валидна
            if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}