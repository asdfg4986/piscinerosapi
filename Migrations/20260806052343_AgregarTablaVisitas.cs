using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinerosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaVisitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    TecnicoId = table.Column<int>(type: "int", nullable: false),
                    FechaVisita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visitas_Tecnicos_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "Tecnicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_ClienteId",
                table: "Visitas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_TecnicoId",
                table: "Visitas",
                column: "TecnicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Visitas");
        }
    }
}
