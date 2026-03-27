using Microsoft.AspNetCore.Identity;
using FitTrack.Models;

namespace FitTrack.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Seed roles
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        const string adminEmail = "admin@fittrack.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Admin",
                EmailConfirmed = true,
                CreatedOn = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Strength" },
                new() { Name = "Cardio" },
                new() { Name = "Flexibility" },
                new() { Name = "HIIT" },
                new() { Name = "Mobility" },
                new() { Name = "Powerlifting" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed exercises
        if (!context.Exercises.Any())
        {
            var strength = context.Categories.First(c => c.Name == "Strength");
            var cardio = context.Categories.First(c => c.Name == "Cardio");
            var flexibility = context.Categories.First(c => c.Name == "Flexibility");
            var hiit = context.Categories.First(c => c.Name == "HIIT");
            var powerlifting = context.Categories.First(c => c.Name == "Powerlifting");

            var exercises = new List<Exercise>
            {
                new() { Name = "Bench Press", Description = "Classic chest press using a barbell on a flat bench.", MuscleGroup = "Chest", CategoryId = strength.Id },
                new() { Name = "Incline Dumbbell Press", Description = "Upper-chest focused press on an incline bench.", MuscleGroup = "Chest", CategoryId = strength.Id },
                new() { Name = "Squat", Description = "Fundamental lower-body compound movement.", MuscleGroup = "Legs", CategoryId = strength.Id },
                new() { Name = "Romanian Deadlift", Description = "Hip-hinge movement targeting hamstrings and glutes.", MuscleGroup = "Hamstrings", CategoryId = strength.Id },
                new() { Name = "Pull-Up", Description = "Bodyweight vertical pull targeting back and biceps.", MuscleGroup = "Back", CategoryId = strength.Id },
                new() { Name = "Overhead Press", Description = "Barbell or dumbbell press overhead targeting shoulders.", MuscleGroup = "Shoulders", CategoryId = strength.Id },
                new() { Name = "Deadlift", Description = "King of all lifts — full posterior chain movement.", MuscleGroup = "Back", CategoryId = powerlifting.Id },
                new() { Name = "Barbell Squat", Description = "Competition squat with a barbell on the traps.", MuscleGroup = "Legs", CategoryId = powerlifting.Id },
                new() { Name = "Powerlifting Bench Press", Description = "Competition-style paused bench press.", MuscleGroup = "Chest", CategoryId = powerlifting.Id },
                new() { Name = "Running", Description = "Steady-state aerobic exercise outdoors or on treadmill.", MuscleGroup = "Full Body", CategoryId = cardio.Id },
                new() { Name = "Cycling", Description = "Low-impact cardio on a bike or stationary trainer.", MuscleGroup = "Legs", CategoryId = cardio.Id },
                new() { Name = "Rowing Machine", Description = "Full-body low-impact cardiovascular exercise.", MuscleGroup = "Full Body", CategoryId = cardio.Id },
                new() { Name = "Burpees", Description = "High-intensity full-body plyometric movement.", MuscleGroup = "Full Body", CategoryId = hiit.Id },
                new() { Name = "Jump Rope", Description = "Classic conditioning drill for coordination and cardio.", MuscleGroup = "Full Body", CategoryId = hiit.Id },
                new() { Name = "Box Jumps", Description = "Explosive lower-body power exercise.", MuscleGroup = "Legs", CategoryId = hiit.Id },
                new() { Name = "Hip Flexor Stretch", Description = "Lunge-based stretch opening up the hip flexors.", MuscleGroup = "Hips", CategoryId = flexibility.Id },
                new() { Name = "Hamstring Stretch", Description = "Seated or standing stretch for the posterior chain.", MuscleGroup = "Hamstrings", CategoryId = flexibility.Id },
                new() { Name = "Thoracic Rotation", Description = "Mobility drill for mid-spine rotation.", MuscleGroup = "Spine", CategoryId = flexibility.Id }
            };
            context.Exercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }
    }
}
