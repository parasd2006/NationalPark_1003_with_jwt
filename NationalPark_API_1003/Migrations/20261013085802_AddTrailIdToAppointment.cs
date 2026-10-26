using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NationalPark_API_1003.Migrations
{
    /// <inheritdoc />
    public partial class AddTrailIdToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrailId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TrailId",
                table: "Appointments",
                column: "TrailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Trails_TrailId",
                table: "Appointments",
                column: "TrailId",
                principalTable: "Trails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Trails_TrailId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TrailId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TrailId",
                table: "Appointments");
        }
    }
}
