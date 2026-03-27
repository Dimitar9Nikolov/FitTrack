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

        // Seed exercises — idempotent: only add exercises that don't exist yet
        {
            var strength     = context.Categories.First(c => c.Name == "Strength");
            var cardio       = context.Categories.First(c => c.Name == "Cardio");
            var flexibility  = context.Categories.First(c => c.Name == "Flexibility");
            var hiit         = context.Categories.First(c => c.Name == "HIIT");
            var powerlifting = context.Categories.First(c => c.Name == "Powerlifting");

            var existing = context.Exercises.Select(e => e.Name).ToHashSet();

            var allExercises = new List<Exercise>
            {
                // Strength
                new() { Name = "Bench Press",              Description = "Classic chest press using a barbell on a flat bench.",              MuscleGroup = "Chest",      CategoryId = strength.Id,     ExerciseType = "Strength" },
                new() { Name = "Incline Dumbbell Press",   Description = "Upper-chest focused press on an incline bench.",                    MuscleGroup = "Chest",      CategoryId = strength.Id,     ExerciseType = "Strength" },
                new() { Name = "Squat",                    Description = "Fundamental lower-body compound movement.",                         MuscleGroup = "Legs",       CategoryId = strength.Id,     ExerciseType = "Strength" },
                new() { Name = "Romanian Deadlift",        Description = "Hip-hinge movement targeting hamstrings and glutes.",               MuscleGroup = "Hamstrings", CategoryId = strength.Id,     ExerciseType = "Strength" },
                new() { Name = "Pull-Up",                  Description = "Bodyweight vertical pull targeting back and biceps.",               MuscleGroup = "Back",       CategoryId = strength.Id,     ExerciseType = "Strength" },
                new() { Name = "Overhead Press",           Description = "Barbell or dumbbell press overhead targeting shoulders.",           MuscleGroup = "Shoulders",  CategoryId = strength.Id,     ExerciseType = "Strength" },
                new() { Name = "Dumbbell Row",             Description = "Unilateral back exercise with a dumbbell.",                         MuscleGroup = "Back",       CategoryId = strength.Id,     ExerciseType = "Strength" },
                // Powerlifting
                new() { Name = "Deadlift",                 Description = "King of all lifts — full posterior chain movement.",                MuscleGroup = "Back",       CategoryId = powerlifting.Id, ExerciseType = "Strength" },
                new() { Name = "Barbell Squat",            Description = "Competition squat with a barbell on the traps.",                    MuscleGroup = "Legs",       CategoryId = powerlifting.Id, ExerciseType = "Strength" },
                new() { Name = "Powerlifting Bench Press", Description = "Competition-style paused bench press.",                            MuscleGroup = "Chest",      CategoryId = powerlifting.Id, ExerciseType = "Strength" },
                // Cardio
                new() { Name = "Running",                  Description = "Steady-state aerobic exercise outdoors or on treadmill.",           MuscleGroup = "Full Body",  CategoryId = cardio.Id,       ExerciseType = "Cardio" },
                new() { Name = "Cycling",                  Description = "Low-impact cardio on a bike or stationary trainer.",                MuscleGroup = "Legs",       CategoryId = cardio.Id,       ExerciseType = "Cardio" },
                new() { Name = "Rowing Machine",           Description = "Full-body low-impact cardiovascular exercise.",                     MuscleGroup = "Full Body",  CategoryId = cardio.Id,       ExerciseType = "Cardio" },
                // HIIT
                new() { Name = "Burpees",                  Description = "High-intensity full-body plyometric movement.",                     MuscleGroup = "Full Body",  CategoryId = hiit.Id,         ExerciseType = "Strength" },
                new() { Name = "Jump Rope",                Description = "Classic conditioning drill for coordination and cardio.",            MuscleGroup = "Full Body",  CategoryId = hiit.Id,         ExerciseType = "Cardio" },
                new() { Name = "Box Jumps",                Description = "Explosive lower-body power exercise.",                              MuscleGroup = "Legs",       CategoryId = hiit.Id,         ExerciseType = "Strength" },
                new() { Name = "Mountain Climbers",        Description = "Core and cardio drill performed in a plank position.",              MuscleGroup = "Core",       CategoryId = hiit.Id,         ExerciseType = "Strength" },
                // Flexibility
                new() { Name = "Hip Flexor Stretch",       Description = "Lunge-based stretch opening up the hip flexors.",                  MuscleGroup = "Hips",       CategoryId = flexibility.Id,  ExerciseType = "Flexibility" },
                new() { Name = "Hamstring Stretch",        Description = "Seated or standing stretch for the posterior chain.",              MuscleGroup = "Hamstrings", CategoryId = flexibility.Id,  ExerciseType = "Flexibility" },
                new() { Name = "Thoracic Rotation",        Description = "Mobility drill for mid-spine rotation.",                           MuscleGroup = "Spine",      CategoryId = flexibility.Id,  ExerciseType = "Flexibility" },
                new() { Name = "Pigeon Pose",              Description = "Deep hip-opener targeting the glutes and hip rotators.",            MuscleGroup = "Hips",       CategoryId = flexibility.Id,  ExerciseType = "Flexibility" },
                new() { Name = "Shoulder Stretch",         Description = "Cross-body or doorway stretch for the shoulders.",                 MuscleGroup = "Shoulders",  CategoryId = flexibility.Id,  ExerciseType = "Flexibility" }
            };

            var toAdd = allExercises.Where(e => !existing.Contains(e.Name)).ToList();
            if (toAdd.Any())
            {
                context.Exercises.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        // Fix ExerciseType for existing exercises that were seeded before this field existed
        var cardioNames = new HashSet<string> { "Running", "Cycling", "Rowing Machine", "Jump Rope" };
        var flexibilityNames = new HashSet<string> { "Hip Flexor Stretch", "Hamstring Stretch", "Thoracic Rotation", "Pigeon Pose", "Shoulder Stretch" };
        var exercisesWithWrongType = context.Exercises.Where(e => e.ExerciseType == "").ToList();
        foreach (var e in exercisesWithWrongType)
        {
            e.ExerciseType = cardioNames.Contains(e.Name) ? "Cardio"
                : flexibilityNames.Contains(e.Name) ? "Flexibility"
                : "Strength";
        }
        if (exercisesWithWrongType.Any())
            await context.SaveChangesAsync();

        // Seed workout plans
        if (!context.WorkoutPlans.Any())
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null) return;

            // Load exercises by name for easy reference
            var ex = context.Exercises.ToDictionary(e => e.Name);

            var plans = new List<WorkoutPlan>
            {
                new()
                {
                    Title = "Beginner Full Body",
                    Description = "A simple 3-day full-body routine perfect for beginners. Covers all major muscle groups with compound movements, keeping volume low to build the habit of training.",
                    Difficulty = "Beginner",
                    IsPublic = true,
                    CreatorId = adminUser.Id,
                    PlanExercises = new List<WorkoutPlanExercise>
                    {
                        new() { ExerciseId = ex["Squat"].Id,            Sets = 3, Reps = 10, OrderIndex = 1, Notes = "Keep chest up, knees over toes" },
                        new() { ExerciseId = ex["Bench Press"].Id,      Sets = 3, Reps = 10, OrderIndex = 2, Notes = "Control the descent, full range of motion" },
                        new() { ExerciseId = ex["Dumbbell Row"].Id,     Sets = 3, Reps = 10, OrderIndex = 3, Notes = "Keep back flat, drive elbow to hip" },
                        new() { ExerciseId = ex["Overhead Press"].Id,   Sets = 3, Reps = 10, OrderIndex = 4 },
                        new() { ExerciseId = ex["Romanian Deadlift"].Id,Sets = 3, Reps = 10, OrderIndex = 5, Notes = "Hinge at hips, slight knee bend" },
                        new() { ExerciseId = ex["Running"].Id,          Sets = 1, Reps = 1,  OrderIndex = 6, Notes = "10–15 min easy cardio to finish" }
                    }
                },
                new()
                {
                    Title = "5-Day Strength Split",
                    Description = "A classic Push/Pull/Legs split for intermediate lifters. Each session focuses on progressive overload with compound barbell and dumbbell movements.",
                    Difficulty = "Intermediate",
                    IsPublic = true,
                    CreatorId = adminUser.Id,
                    PlanExercises = new List<WorkoutPlanExercise>
                    {
                        new() { ExerciseId = ex["Bench Press"].Id,           Sets = 4, Reps = 8, OrderIndex = 1, Notes = "Day 1 – Push" },
                        new() { ExerciseId = ex["Incline Dumbbell Press"].Id, Sets = 3, Reps = 10, OrderIndex = 2 },
                        new() { ExerciseId = ex["Overhead Press"].Id,         Sets = 4, Reps = 8, OrderIndex = 3 },
                        new() { ExerciseId = ex["Pull-Up"].Id,                Sets = 4, Reps = 8, OrderIndex = 4, Notes = "Day 2 – Pull" },
                        new() { ExerciseId = ex["Dumbbell Row"].Id,           Sets = 4, Reps = 10, OrderIndex = 5 },
                        new() { ExerciseId = ex["Deadlift"].Id,               Sets = 3, Reps = 6, OrderIndex = 6 },
                        new() { ExerciseId = ex["Squat"].Id,                  Sets = 4, Reps = 8, OrderIndex = 7, Notes = "Day 3 – Legs" },
                        new() { ExerciseId = ex["Romanian Deadlift"].Id,      Sets = 3, Reps = 10, OrderIndex = 8 }
                    }
                },
                new()
                {
                    Title = "Advanced Powerlifting Program",
                    Description = "Competition-prep style program focused on squat, bench and deadlift. Periodised with heavy singles, doubles and triples. Not for beginners.",
                    Difficulty = "Advanced",
                    IsPublic = true,
                    CreatorId = adminUser.Id,
                    PlanExercises = new List<WorkoutPlanExercise>
                    {
                        new() { ExerciseId = ex["Barbell Squat"].Id,          Sets = 5, Reps = 3, OrderIndex = 1, Notes = "Work up to heavy triple" },
                        new() { ExerciseId = ex["Powerlifting Bench Press"].Id,Sets = 5, Reps = 3, OrderIndex = 2, Notes = "Paused reps, leg drive" },
                        new() { ExerciseId = ex["Deadlift"].Id,                Sets = 5, Reps = 3, OrderIndex = 3, Notes = "Conventional or sumo" },
                        new() { ExerciseId = ex["Romanian Deadlift"].Id,       Sets = 4, Reps = 5, OrderIndex = 4, Notes = "Accessory for hamstrings" },
                        new() { ExerciseId = ex["Pull-Up"].Id,                 Sets = 4, Reps = 6, OrderIndex = 5, Notes = "Weighted if possible" },
                        new() { ExerciseId = ex["Overhead Press"].Id,          Sets = 3, Reps = 8, OrderIndex = 6 }
                    }
                },
                new()
                {
                    Title = "HIIT Cardio Blast",
                    Description = "High-intensity interval training with bodyweight movements and jump rope. 4 days a week, 30–40 minutes per session. Great for fat loss and conditioning.",
                    Difficulty = "Intermediate",
                    IsPublic = true,
                    CreatorId = adminUser.Id,
                    PlanExercises = new List<WorkoutPlanExercise>
                    {
                        new() { ExerciseId = ex["Jump Rope"].Id,        Sets = 4, Reps = 1, OrderIndex = 1, Notes = "3 min on / 1 min rest" },
                        new() { ExerciseId = ex["Burpees"].Id,          Sets = 4, Reps = 15, OrderIndex = 2, Notes = "Full range, jump at top" },
                        new() { ExerciseId = ex["Box Jumps"].Id,        Sets = 4, Reps = 10, OrderIndex = 3, Notes = "Land softly, step down" },
                        new() { ExerciseId = ex["Mountain Climbers"].Id,Sets = 4, Reps = 20, OrderIndex = 4, Notes = "20 reps each leg" },
                        new() { ExerciseId = ex["Running"].Id,          Sets = 1, Reps = 1,  OrderIndex = 5, Notes = "10 min cool-down jog" }
                    }
                },
                new()
                {
                    Title = "Flexibility & Mobility Routine",
                    Description = "A daily stretching and mobility program to improve range of motion, reduce injury risk and aid recovery. Ideal standalone or alongside any strength program.",
                    Difficulty = "Beginner",
                    IsPublic = true,
                    CreatorId = adminUser.Id,
                    PlanExercises = new List<WorkoutPlanExercise>
                    {
                        new() { ExerciseId = ex["Hip Flexor Stretch"].Id, Sets = 3, Reps = 1, OrderIndex = 1, Notes = "Hold 30 seconds each side" },
                        new() { ExerciseId = ex["Hamstring Stretch"].Id,  Sets = 3, Reps = 1, OrderIndex = 2, Notes = "Hold 30 seconds each side" },
                        new() { ExerciseId = ex["Pigeon Pose"].Id,        Sets = 2, Reps = 1, OrderIndex = 3, Notes = "Hold 45 seconds each side" },
                        new() { ExerciseId = ex["Thoracic Rotation"].Id,  Sets = 3, Reps = 10, OrderIndex = 4, Notes = "5 each side" },
                        new() { ExerciseId = ex["Shoulder Stretch"].Id,   Sets = 3, Reps = 1, OrderIndex = 5, Notes = "Hold 30 seconds each side" }
                    }
                },
                new()
                {
                    Title = "12-Week Body Recomposition",
                    Description = "A 4-day program combining strength training and metabolic conditioning to build muscle and lose fat simultaneously. Best paired with a maintenance or slight deficit diet.",
                    Difficulty = "Intermediate",
                    IsPublic = true,
                    CreatorId = adminUser.Id,
                    PlanExercises = new List<WorkoutPlanExercise>
                    {
                        new() { ExerciseId = ex["Squat"].Id,             Sets = 4, Reps = 8,  OrderIndex = 1 },
                        new() { ExerciseId = ex["Bench Press"].Id,       Sets = 4, Reps = 8,  OrderIndex = 2 },
                        new() { ExerciseId = ex["Deadlift"].Id,          Sets = 3, Reps = 6,  OrderIndex = 3 },
                        new() { ExerciseId = ex["Pull-Up"].Id,           Sets = 4, Reps = 8,  OrderIndex = 4 },
                        new() { ExerciseId = ex["Overhead Press"].Id,    Sets = 3, Reps = 10, OrderIndex = 5 },
                        new() { ExerciseId = ex["Burpees"].Id,           Sets = 3, Reps = 15, OrderIndex = 6, Notes = "Metabolic finisher" },
                        new() { ExerciseId = ex["Rowing Machine"].Id,    Sets = 1, Reps = 1,  OrderIndex = 7, Notes = "20 min steady state" }
                    }
                }
            };

            context.WorkoutPlans.AddRange(plans);
            await context.SaveChangesAsync();
        }

        // Seed plan exercises for existing plans that have none
        if (context.WorkoutPlans.Any() && !context.WorkoutPlanExercises.Any())
        {
            var ex = context.Exercises.ToDictionary(e => e.Name);
            var plans = context.WorkoutPlans.ToList();

            var planMap = plans.ToDictionary(p => p.Title);

            var planExercises = new List<WorkoutPlanExercise>();

            if (planMap.TryGetValue("Beginner Full Body", out var p1))
                planExercises.AddRange(new[]
                {
                    new WorkoutPlanExercise { WorkoutPlanId = p1.Id, ExerciseId = ex["Squat"].Id,             Sets = 3, Reps = 10, OrderIndex = 1, Notes = "Keep chest up, knees over toes" },
                    new WorkoutPlanExercise { WorkoutPlanId = p1.Id, ExerciseId = ex["Bench Press"].Id,       Sets = 3, Reps = 10, OrderIndex = 2 },
                    new WorkoutPlanExercise { WorkoutPlanId = p1.Id, ExerciseId = ex["Dumbbell Row"].Id,      Sets = 3, Reps = 10, OrderIndex = 3 },
                    new WorkoutPlanExercise { WorkoutPlanId = p1.Id, ExerciseId = ex["Overhead Press"].Id,    Sets = 3, Reps = 10, OrderIndex = 4 },
                    new WorkoutPlanExercise { WorkoutPlanId = p1.Id, ExerciseId = ex["Romanian Deadlift"].Id, Sets = 3, Reps = 10, OrderIndex = 5 },
                    new WorkoutPlanExercise { WorkoutPlanId = p1.Id, ExerciseId = ex["Running"].Id,           Sets = 1, Reps = 1,  OrderIndex = 6, Notes = "10–15 min easy cardio" }
                });

            if (planMap.TryGetValue("5-Day Strength Split", out var p2))
                planExercises.AddRange(new[]
                {
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Bench Press"].Id,            Sets = 4, Reps = 8,  OrderIndex = 1, Notes = "Day 1 – Push" },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Incline Dumbbell Press"].Id,  Sets = 3, Reps = 10, OrderIndex = 2 },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Overhead Press"].Id,          Sets = 4, Reps = 8,  OrderIndex = 3 },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Pull-Up"].Id,                 Sets = 4, Reps = 8,  OrderIndex = 4, Notes = "Day 2 – Pull" },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Dumbbell Row"].Id,            Sets = 4, Reps = 10, OrderIndex = 5 },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Deadlift"].Id,                Sets = 3, Reps = 6,  OrderIndex = 6 },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Squat"].Id,                   Sets = 4, Reps = 8,  OrderIndex = 7, Notes = "Day 3 – Legs" },
                    new WorkoutPlanExercise { WorkoutPlanId = p2.Id, ExerciseId = ex["Romanian Deadlift"].Id,       Sets = 3, Reps = 10, OrderIndex = 8 }
                });

            if (planMap.TryGetValue("Advanced Powerlifting Program", out var p3))
                planExercises.AddRange(new[]
                {
                    new WorkoutPlanExercise { WorkoutPlanId = p3.Id, ExerciseId = ex["Barbell Squat"].Id,           Sets = 5, Reps = 3, OrderIndex = 1, Notes = "Work up to heavy triple" },
                    new WorkoutPlanExercise { WorkoutPlanId = p3.Id, ExerciseId = ex["Powerlifting Bench Press"].Id, Sets = 5, Reps = 3, OrderIndex = 2, Notes = "Paused reps" },
                    new WorkoutPlanExercise { WorkoutPlanId = p3.Id, ExerciseId = ex["Deadlift"].Id,                 Sets = 5, Reps = 3, OrderIndex = 3 },
                    new WorkoutPlanExercise { WorkoutPlanId = p3.Id, ExerciseId = ex["Romanian Deadlift"].Id,        Sets = 4, Reps = 5, OrderIndex = 4 },
                    new WorkoutPlanExercise { WorkoutPlanId = p3.Id, ExerciseId = ex["Pull-Up"].Id,                  Sets = 4, Reps = 6, OrderIndex = 5, Notes = "Weighted if possible" },
                    new WorkoutPlanExercise { WorkoutPlanId = p3.Id, ExerciseId = ex["Overhead Press"].Id,           Sets = 3, Reps = 8, OrderIndex = 6 }
                });

            if (planMap.TryGetValue("HIIT Cardio Blast", out var p4))
                planExercises.AddRange(new[]
                {
                    new WorkoutPlanExercise { WorkoutPlanId = p4.Id, ExerciseId = ex["Jump Rope"].Id,         Sets = 4, Reps = 1,  OrderIndex = 1, Notes = "3 min on / 1 min rest" },
                    new WorkoutPlanExercise { WorkoutPlanId = p4.Id, ExerciseId = ex["Burpees"].Id,           Sets = 4, Reps = 15, OrderIndex = 2 },
                    new WorkoutPlanExercise { WorkoutPlanId = p4.Id, ExerciseId = ex["Box Jumps"].Id,         Sets = 4, Reps = 10, OrderIndex = 3 },
                    new WorkoutPlanExercise { WorkoutPlanId = p4.Id, ExerciseId = ex["Mountain Climbers"].Id, Sets = 4, Reps = 20, OrderIndex = 4, Notes = "20 reps each leg" },
                    new WorkoutPlanExercise { WorkoutPlanId = p4.Id, ExerciseId = ex["Running"].Id,           Sets = 1, Reps = 1,  OrderIndex = 5, Notes = "10 min cool-down jog" }
                });

            if (planMap.TryGetValue("Flexibility & Mobility Routine", out var p5))
                planExercises.AddRange(new[]
                {
                    new WorkoutPlanExercise { WorkoutPlanId = p5.Id, ExerciseId = ex["Hip Flexor Stretch"].Id, Sets = 3, Reps = 1, OrderIndex = 1, Notes = "Hold 30 seconds each side" },
                    new WorkoutPlanExercise { WorkoutPlanId = p5.Id, ExerciseId = ex["Hamstring Stretch"].Id,  Sets = 3, Reps = 1, OrderIndex = 2, Notes = "Hold 30 seconds each side" },
                    new WorkoutPlanExercise { WorkoutPlanId = p5.Id, ExerciseId = ex["Pigeon Pose"].Id,        Sets = 2, Reps = 1, OrderIndex = 3, Notes = "Hold 45 seconds each side" },
                    new WorkoutPlanExercise { WorkoutPlanId = p5.Id, ExerciseId = ex["Thoracic Rotation"].Id,  Sets = 3, Reps = 10, OrderIndex = 4, Notes = "5 each side" },
                    new WorkoutPlanExercise { WorkoutPlanId = p5.Id, ExerciseId = ex["Shoulder Stretch"].Id,   Sets = 3, Reps = 1, OrderIndex = 5, Notes = "Hold 30 seconds each side" }
                });

            if (planMap.TryGetValue("12-Week Body Recomposition", out var p6))
                planExercises.AddRange(new[]
                {
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Squat"].Id,          Sets = 4, Reps = 8,  OrderIndex = 1 },
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Bench Press"].Id,    Sets = 4, Reps = 8,  OrderIndex = 2 },
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Deadlift"].Id,       Sets = 3, Reps = 6,  OrderIndex = 3 },
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Pull-Up"].Id,        Sets = 4, Reps = 8,  OrderIndex = 4 },
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Overhead Press"].Id, Sets = 3, Reps = 10, OrderIndex = 5 },
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Burpees"].Id,        Sets = 3, Reps = 15, OrderIndex = 6, Notes = "Metabolic finisher" },
                    new WorkoutPlanExercise { WorkoutPlanId = p6.Id, ExerciseId = ex["Rowing Machine"].Id, Sets = 1, Reps = 1,  OrderIndex = 7, Notes = "20 min steady state" }
                });

            if (planExercises.Any())
            {
                context.WorkoutPlanExercises.AddRange(planExercises);
                await context.SaveChangesAsync();
            }
        }
    }
}
