using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseTypeAndCardioFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardioMinutes",
                table: "WorkoutExercises",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Incline",
                table: "WorkoutExercises",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SpeedKmh",
                table: "WorkoutExercises",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExerciseType",
                table: "Exercises",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardioMinutes",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "Incline",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "SpeedKmh",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "ExerciseType",
                table: "Exercises");
        }
    }
}
