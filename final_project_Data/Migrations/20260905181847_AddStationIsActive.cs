using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace final_project_Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStationIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ChargingStations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ChargingStations");
        }
    }
}
