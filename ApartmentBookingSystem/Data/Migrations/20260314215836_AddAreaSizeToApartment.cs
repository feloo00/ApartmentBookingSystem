using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmentBookingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaSizeToApartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ApartmentSize",
                table: "Apartments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "LocationUrl",
                table: "Apartments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentSize",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "LocationUrl",
                table: "Apartments");
        }
    }
}
