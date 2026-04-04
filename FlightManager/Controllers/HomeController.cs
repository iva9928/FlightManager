using System.Diagnostics;
using FlightManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightManager.Controllers
{
    /// <summary>
    /// Контролер за основните страници на приложението — начална страница, поверителност и грешки.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Инициализира нов екземпляр на <see cref="HomeController"/>.
        /// </summary>
        /// <param name="logger">Услуга за логване.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Показва началната страница на приложението.
        /// </summary>
        /// <returns>Изглед на началната страница.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Показва страницата с политика за поверителност.
        /// </summary>
        /// <returns>Изглед на страницата за поверителност.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Показва страницата за грешки с идентификатор на заявката.
        /// Отговорът не се кешира, за да се гарантира актуална информация за грешката.
        /// </summary>
        /// <returns>Изглед за грешка с модел, съдържащ идентификатора на заявката.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}