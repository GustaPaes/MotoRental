using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRentalEntitie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnDate",
                table: "Rentals",
                newName: "ActualEndDate");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalTotalCost",
                table: "Rentals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Rentals",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalTotalCost",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "ActualEndDate",
                table: "Rentals",
                newName: "ReturnDate");
        }
    }
}
