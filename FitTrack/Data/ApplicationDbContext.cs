using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitTrack.Models;

namespace FitTrack.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
    public DbSet<WorkoutExerciseSet> WorkoutExerciseSets { get; set; }
    public DbSet<WorkoutPlanExercise> WorkoutPlanExercises { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<WorkoutExercise>()
            .Property(we => we.SpeedKmh)
            .HasColumnType("decimal(5,1)");

        builder.Entity<WorkoutExercise>()
            .Property(we => we.Incline)
            .HasColumnType("decimal(4,1)");

        builder.Entity<Workout>()
            .HasOne(w => w.WorkoutPlan)
            .WithMany(wp => wp.Workouts)
            .HasForeignKey(w => w.WorkoutPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Workout>()
            .HasOne(w => w.User)
            .WithMany(u => u.Workouts)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WorkoutPlan>()
            .HasOne(wp => wp.Creator)
            .WithMany(u => u.WorkoutPlans)
            .HasForeignKey(wp => wp.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WorkoutPlanExercise>()
            .HasOne(wpe => wpe.WorkoutPlan)
            .WithMany(wp => wp.PlanExercises)
            .HasForeignKey(wpe => wpe.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WorkoutPlanExercise>()
            .HasOne(wpe => wpe.Exercise)
            .WithMany(e => e.WorkoutPlanExercises)
            .HasForeignKey(wpe => wpe.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
