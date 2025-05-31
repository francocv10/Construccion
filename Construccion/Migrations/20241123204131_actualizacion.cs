using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Construccion.Migrations
{
    /// <inheritdoc />
    public partial class actualizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_Obra",
                table: "Insumos");

            migrationBuilder.DropIndex(
                name: "IX_Insumos_IdObra",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "IdObra",
                table: "Insumos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdObra",
                table: "Insumos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_IdObra",
                table: "Insumos",
                column: "IdObra");

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_Obra",
                table: "Insumos",
                column: "IdObra",
                principalTable: "Obras",
                principalColumn: "IdObra",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
