using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace FlightManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

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

            var items = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Search = search;

            return View(items);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }
    }
}