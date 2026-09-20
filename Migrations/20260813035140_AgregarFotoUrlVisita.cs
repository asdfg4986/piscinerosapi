using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinerosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFotoUrlVisita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Visitas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Visitas");
        }
    }
}
