using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinerosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroClienteLegacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroClienteLegacy",
                table: "Clientes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NumeroClienteLegacy",
                table: "Clientes",
                column: "NumeroClienteLegacy",
                unique: true,
                filter: "[NumeroClienteLegacy] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_NumeroClienteLegacy",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumeroClienteLegacy",
                table: "Clientes");
        }
    }
}
