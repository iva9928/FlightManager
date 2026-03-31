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
            // 🔥 ROLES
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Employee"))
                await roleManager.CreateAsync(new IdentityRole("Employee"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // 🔴 ADMIN
            var adminEmail = "admin@flightmanager.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EGN = "0000000000",
                    Address = "Sofia",
                    PhoneNumber = "000000000"
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 🔵 EMPLOYEE
            var employeeEmail = "employee@flightmanager.com";

            var employee = await userManager.FindByEmailAsync(employeeEmail);

            if (employee == null)
            {
                employee = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    FirstName = "Flight",
                    LastName = "Employee",
                    EGN = "1111111111",
                    Address = "Plovdiv",
                    PhoneNumber = "111111111"
                };

                var result = await userManager.CreateAsync(employee, "Employee123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employee, "Employee");
                }
            }

            // 🟡 USER
            var userEmail = "user@flightmanager.com";

            var user = await userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = "Normal",
                    LastName = "User",
                    EGN = "2222222222",
                    Address = "Varna",
                    PhoneNumber = "222222222"
                };

                var result = await userManager.CreateAsync(user, "User123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }
    }
}