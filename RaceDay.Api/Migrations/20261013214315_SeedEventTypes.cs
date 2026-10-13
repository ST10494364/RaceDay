using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedEventTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EventType",
                columns: new[] { "event_type_id", "type_name" },
                values: new object[,]
                {
                    { 1, "Run" },
                    { 2, "Walk" },
                    { 3, "Cycle" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EventType",
                keyColumn: "event_type_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EventType",
                keyColumn: "event_type_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EventType",
                keyColumn: "event_type_id",
                keyValue: 3);
        }
    }
}
