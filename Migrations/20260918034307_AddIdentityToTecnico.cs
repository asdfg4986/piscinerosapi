using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinerosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityToTecnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Tecnicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Tecnicos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Tecnicos");
        }
    }
}
