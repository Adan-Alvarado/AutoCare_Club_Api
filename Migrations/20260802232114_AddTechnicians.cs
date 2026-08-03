using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCare_Club.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicians : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_users_TechnicianId",
                table: "Appointments");

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Specialty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technicians", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Technicians_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Technicians_TechnicianId",
                table: "Appointments",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Technicians_TechnicianId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Technicians");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_users_TechnicianId",
                table: "Appointments",
                column: "TechnicianId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
