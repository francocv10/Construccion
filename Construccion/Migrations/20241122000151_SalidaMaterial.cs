using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Construccion.Migrations
{
    /// <inheritdoc />
    public partial class SalidaMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdObra",
                table: "SalidaMaterial",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalidaMaterial_IdObra",
                table: "SalidaMaterial",
                column: "IdObra");

            migrationBuilder.AddForeignKey(
                name: "FK_SalidaMaterial_Obra",
                table: "SalidaMaterial",
                column: "IdObra",
                principalTable: "Obras",
                principalColumn: "IdObra",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalidaMaterial_Obra",
                table: "SalidaMaterial");

            migrationBuilder.DropIndex(
                name: "IX_SalidaMaterial_IdObra",
                table: "SalidaMaterial");

            migrationBuilder.DropColumn(
                name: "IdObra",
                table: "SalidaMaterial");
        }
    }
}
