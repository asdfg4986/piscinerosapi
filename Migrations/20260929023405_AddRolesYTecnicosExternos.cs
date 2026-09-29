using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinerosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesYTecnicosExternos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsExterno",
                table: "Tecnicos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TecnicoExternoId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TecnicoExternoId",
                table: "Clientes",
                column: "TecnicoExternoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Tecnicos_TecnicoExternoId",
                table: "Clientes",
                column: "TecnicoExternoId",
                principalTable: "Tecnicos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Tecnicos_TecnicoExternoId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_TecnicoExternoId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EsExterno",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "TecnicoExternoId",
                table: "Clientes");
        }
    }
}
