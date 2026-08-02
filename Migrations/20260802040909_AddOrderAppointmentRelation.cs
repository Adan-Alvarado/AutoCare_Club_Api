using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCare_Club.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAppointmentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_AppointmentId",
                table: "Orders",
                column: "AppointmentId",
                unique: true,
                filter: "\"AppointmentId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Appointments_AppointmentId",
                table: "Orders",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Appointments_AppointmentId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AppointmentId",
                table: "Orders");
        }
    }
}
