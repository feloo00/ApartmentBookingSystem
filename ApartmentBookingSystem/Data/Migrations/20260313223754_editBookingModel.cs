using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmentBookingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class editBookingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentProofImageUrl",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentProofImageUrl",
                table: "Bookings");
        }
    }
}
