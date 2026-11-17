
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaGo.Infrastructure.Migrations
{
    public partial class UpdateTicketEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Prevent truncation of existing ticket numbers.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM dbo.Tickets
                    WHERE LEN(TicketNumber) > 50
                )
                BEGIN
                    THROW 50001,
                        'Migration stopped: TicketNumber exceeds 50 characters.',
                        1;
                END
            ");

            // Ticket Number: max 50 characters
            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "Tickets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Ticket Price
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Tickets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Ticket Type
            migrationBuilder.AddColumn<string>(
                name: "TicketType",
                table: "Tickets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Adult");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketType",
                table: "Tickets");

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
