using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Construccion.Migrations
{
    /// <inheritdoc />
    public partial class Hitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hito_Obra",
                table: "Hitos");

            migrationBuilder.AddColumn<int>(
                name: "IdPartida",
                table: "Hitos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_IdPartida",
                table: "Hitos",
                column: "IdPartida");

            migrationBuilder.AddForeignKey(
                name: "FK_Hito_Obra",
                table: "Hitos",
                column: "IdObra",
                principalTable: "Obras",
                principalColumn: "IdObra",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Hito_Partida",
                table: "Hitos",
                column: "IdPartida",
                principalTable: "Partida",
                principalColumn: "IdPartida",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hito_Obra",
                table: "Hitos");

            migrationBuilder.DropForeignKey(
                name: "FK_Hito_Partida",
                table: "Hitos");

            migrationBuilder.DropIndex(
                name: "IX_Hitos_IdPartida",
                table: "Hitos");

            migrationBuilder.DropColumn(
                name: "IdPartida",
                table: "Hitos");

            migrationBuilder.AddForeignKey(
                name: "FK_Hito_Obra",
                table: "Hitos",
                column: "IdObra",
                principalTable: "Obras",
                principalColumn: "IdObra",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
