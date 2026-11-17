using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservationForSeatHold : Migration
    {
        /// <inheritdoc />
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_SessionId_SeatId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ReserveAt",
                table: "Reservations",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Confirmed");

            migrationBuilder.Sql("""
                                 UPDATE Reservations
                                 SET Status = CASE
                                     WHEN IsCancelled = 1 THEN 'Cancelled'
                                     ELSE 'Confirmed'
                                 END
                                 """);

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SessionId_SeatId",
                table: "Reservations",
                columns: new[] { "SessionId", "SeatId" });
        }


        /// <inheritdoc />
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_SessionId_SeatId",
                table: "Reservations");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                                 UPDATE Reservations
                                 SET IsCancelled = CASE
                                     WHEN Status = 'Cancelled' THEN 1
                                     ELSE 0
                                 END
                                 """);

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reservations",
                newName: "ReserveAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SessionId_SeatId",
                table: "Reservations",
                columns: new[] { "SessionId", "SeatId" },
                unique: true,
                filter: "[IsCancelled] = 0");
        }

    }
}
