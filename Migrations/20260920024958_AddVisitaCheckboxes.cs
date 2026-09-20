using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinerosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitaCheckboxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aspirado",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Canastillos",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Cepillado",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Cloro",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Llaves",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Llenando",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ph",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Retrolavado",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aspirado",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Canastillos",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Cepillado",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Cloro",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Llaves",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Llenando",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Ph",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Retrolavado",
                table: "Visitas");
        }
    }
}
