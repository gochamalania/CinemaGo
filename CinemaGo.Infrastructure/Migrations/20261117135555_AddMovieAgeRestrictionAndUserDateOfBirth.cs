using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieAgeRestrictionAndUserDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeRestriction",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AgeRestriction",
                table: "Movies");
        }
    }
}
