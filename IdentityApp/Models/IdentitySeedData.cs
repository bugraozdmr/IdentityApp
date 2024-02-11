using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models;

public static class IdentitySeedData
{
    private const string adminUser = "Admin";
    private const string AdminPassword = "Admin123";

    public static async void IdentityTestUser(IApplicationBuilder app)
    {
        var context = app.ApplicationServices
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IdentityContext>();

        // bekleyen migration varsa direkt al
        if (context.Database.GetAppliedMigrations().Any())
        {
            context.Database.Migrate();
        }

        var userManager = app
            .ApplicationServices
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.FindByNameAsync(adminUser);

        if (user == null)
        {
            user = new IdentityUser()
            {
                UserName = adminUser,
                Email = "admin@gmail.com",
                PhoneNumber = "44444444444"
            };

            await userManager.CreateAsync(user, AdminPassword);
        }
    }
}