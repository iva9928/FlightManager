using FlightManager.Data;
using FlightManager.Data.Models;
using FlightManager.Services.Interfaces;
using FlightManager.Services.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Входна точка на приложението. Конфигурира и стартира уеб сървъра.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Конфигурация на базата данни
/// <summary>
/// Извлича connection string от конфигурационния файл и регистрира DbContext.
/// </summary>
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FlightManagerDbContext>(options =>
    options.UseSqlServer(connectionString));

/// <summary>
/// Конфигурация на Identity системата за управление на потребители и роли.
/// Изисква уникален имейл, минимална дължина на паролата 6 символа,
/// без задължителни специални символи, цифри или главни букви.
/// </summary>
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<FlightManagerDbContext>()
    .AddDefaultTokenProviders();

/// <summary>
/// Регистрация на приложните услуги в контейнера за зависимости (DI).
/// </summary>
/// <remarks>
/// <see cref="IFlightService"/> — управление на полети.
/// <see cref="IReservationService"/> — управление на резервации.
/// <see cref="IUserService"/> — управление на потребители.
/// </remarks>
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IUserService, UserService>();

/// <summary>
/// Регистрира Razor Pages — използва се от Identity UI и някои части на интерфейса.
/// </summary>
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

/// <summary>
/// Инициализация на начални данни — роли и администраторски акаунт.
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.SeedAsync(userManager, roleManager);
}

/// <summary>
/// Конфигурация на middleware за обработка на грешки.
/// В production среда се пренасочва към централизирана страница за грешки.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

/// <summary>
/// Активира middleware за автентикация и авторизация.
/// Редът е важен — автентикацията трябва да предшества авторизацията.
/// </summary>
app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Регистрира маршрута по подразбиране за MVC контролерите.
/// Формат: {контролер}/{действие}/{id?}
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/// <summary>
/// Регистрира Razor Pages маршрути — необходими за Identity UI (вход/регистрация).
/// </summary>
app.MapRazorPages();

app.Run();