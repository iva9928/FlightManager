using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlightManager.Data.Models;
using FlightManager.Models;
using System.Threading.Tasks;

namespace FlightManager.Controllers
{
    /// <summary>
    /// Контролер за управление на акаунти — вход, регистрация и изход на потребители.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="AccountController"/>.
        /// </summary>
        /// <param name="userManager">Услуга за управление на потребители.</param>
        /// <param name="signInManager">Услуга за управление на влизане и излизане.</param>
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Показва страницата за вход.
        /// </summary>
        /// <returns>Изглед за вход.</returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Обработва POST заявка за вход с имейл и парола.
        /// </summary>
        /// <param name="model">Модел с данни за вход.</param>
        /// <returns>
        /// Пренасочване към списъка с полети при успех;
        /// изглед с грешка при невалидни данни.
        /// </returns>
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

            ModelState.AddModelError("", "Невалиден имейл или парола");
            return View(model);
        }

        /// <summary>
        /// Показва страницата за регистрация.
        /// </summary>
        /// <returns>Изглед за регистрация.</returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Обработва POST заявка за регистрация на нов потребител.
        /// Създава акаунт, присвоява роля "User" и влиза автоматично.
        /// </summary>
        /// <param name="model">Модел с данни за регистрация.</param>
        /// <returns>
        /// Пренасочване към списъка с полети при успех;
        /// изглед с грешки при неуспешна регистрация.
        /// </returns>
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
                // Присвояване на роля "User" на новорегистрирания потребител
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

        /// <summary>
        /// Обработва POST заявка за изход от системата.
        /// Изчиства сесията на потребителя и пренасочва към списъка с полети.
        /// </summary>
        /// <returns>Пренасочване към началната страница на полетите.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Flights");
        }
    }
}