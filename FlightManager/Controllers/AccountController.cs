using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Data.Models;
using FlightManager.Models;
using System.Threading.Tasks;

namespace FlightManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 🔐 LOGIN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                false,
                false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Flights");

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        // 📝 REGISTER
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EGN = model.EGN,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 🔥 ВАЖНО – правилната роля
                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Flights");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // 🚪 LOGOUT (POST – правилно)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Flights");
        }
    }
}