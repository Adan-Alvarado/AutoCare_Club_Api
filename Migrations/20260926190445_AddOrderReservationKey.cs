using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCare_Club.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderReservationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReservationKey",
                table: "Orders",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_ReservationKey",
                table: "Orders",
                columns: new[] { "UserId", "ReservationKey" },
                unique: true,
                filter: "\"ReservationKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_ReservationKey",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReservationKey",
                table: "Orders");
        }
    }
}
