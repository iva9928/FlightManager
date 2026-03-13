using FlightManager.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace FlightManager.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Employee"))
            {
                await roleManager.CreateAsync(new IdentityRole("Employee"));
            }

            var adminEmail = "admin@flightmanager.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EGN = "0000000000",
                    Address = "Sofia",
                    PhoneNumber = "000000000"
                };

                await userManager.CreateAsync(admin, "Admin123!");

                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}