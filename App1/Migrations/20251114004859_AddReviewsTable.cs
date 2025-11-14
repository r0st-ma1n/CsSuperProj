using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App1.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LessonId",
                table: "Reviews",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Lessons_LessonId",
                table: "Reviews",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Lessons_LessonId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_LessonId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "Reviews");
        }
    }
}
