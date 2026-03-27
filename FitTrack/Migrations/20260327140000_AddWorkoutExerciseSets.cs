using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutExerciseSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the per-set table
            migrationBuilder.CreateTable(
                name: "WorkoutExerciseSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutExerciseId = table.Column<int>(type: "int", nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(6,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExerciseSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutExerciseSets_WorkoutExercises_WorkoutExerciseId",
                        column: x => x.WorkoutExerciseId,
                        principalTable: "WorkoutExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. Migrate existing aggregate data into per-set rows
            //    For each WorkoutExercise with Sets > 0, expand into N identical rows.
            migrationBuilder.Sql(@"
                INSERT INTO WorkoutExerciseSets (WorkoutExerciseId, SetNumber, Reps, WeightKg)
                SELECT we.Id, n.n, we.Reps, we.WeightKg
                FROM   WorkoutExercises we
                CROSS JOIN (
                    SELECT 1 AS n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL
                    SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL
                    SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
                ) n
                WHERE  n.n <= we.Sets
                  AND  we.Sets > 0
                  AND  we.Reps > 0
            ");

            // 3. Drop the now-redundant scalar strength columns
            migrationBuilder.DropColumn(name: "Reps",     table: "WorkoutExercises");
            migrationBuilder.DropColumn(name: "Sets",     table: "WorkoutExercises");
            migrationBuilder.DropColumn(name: "WeightKg", table: "WorkoutExercises");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExerciseSets_WorkoutExerciseId",
                table: "WorkoutExerciseSets",
                column: "WorkoutExerciseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore scalar columns
            migrationBuilder.AddColumn<int>(
                name: "Sets", table: "WorkoutExercises",
                type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Reps", table: "WorkoutExercises",
                type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg", table: "WorkoutExercises",
                type: "decimal(6,2)", nullable: false, defaultValue: 0m);

            // Roll back per-set data into aggregate columns
            migrationBuilder.Sql(@"
                UPDATE we
                SET    we.Sets    = COALESCE(s.SetCount, 0),
                       we.Reps    = COALESCE(s.FirstReps, 0),
                       we.WeightKg = COALESCE(s.FirstWeight, 0)
                FROM   WorkoutExercises we
                LEFT JOIN (
                    SELECT WorkoutExerciseId,
                           COUNT(*)  AS SetCount,
                           MAX(CASE WHEN SetNumber = 1 THEN Reps     END) AS FirstReps,
                           MAX(CASE WHEN SetNumber = 1 THEN WeightKg END) AS FirstWeight
                    FROM   WorkoutExerciseSets
                    GROUP BY WorkoutExerciseId
                ) s ON s.WorkoutExerciseId = we.Id
            ");

            migrationBuilder.DropTable(name: "WorkoutExerciseSets");
        }
    }
}
