using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirReservationsApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "User_id",
                table: "Reservations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "NumOfSeats",
                table: "Reservations",
                newName: "SeatsReserved");

            migrationBuilder.RenameColumn(
                name: "Flight_id",
                table: "Reservations",
                newName: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_FlightId",
                table: "Reservations",
                column: "FlightId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Flights_FlightId",
                table: "Reservations",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Flights_FlightId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_FlightId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reservations",
                newName: "User_id");

            migrationBuilder.RenameColumn(
                name: "SeatsReserved",
                table: "Reservations",
                newName: "NumOfSeats");

            migrationBuilder.RenameColumn(
                name: "FlightId",
                table: "Reservations",
                newName: "Flight_id");
        }
    }
}
