using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Construccion.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bodega",
                columns: table => new
                {
                    IdBodega = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreBodega = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bodega", x => x.IdBodega);
                });

            migrationBuilder.CreateTable(
                name: "Obras",
                columns: table => new
                {
                    IdObra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreObra = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Cliente = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obra", x => x.IdObra);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    IdInsumos = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IdBodega = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.IdInsumos);
                    table.ForeignKey(
                        name: "FK_Insumos_Bodega",
                        column: x => x.IdBodega,
                        principalTable: "Bodega",
                        principalColumn: "IdBodega",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hitos",
                columns: table => new
                {
                    IdHito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdObra = table.Column<int>(type: "int", nullable: false),
                    NombreHito = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Finalizado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hito", x => x.IdHito);
                    table.ForeignKey(
                        name: "FK_Hito_Obra",
                        column: x => x.IdObra,
                        principalTable: "Obras",
                        principalColumn: "IdObra",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Partida",
                columns: table => new
                {
                    IdPartida = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ManoDeObra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdObra = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partida", x => x.IdPartida);
                    table.ForeignKey(
                        name: "FK_Partida_Obra",
                        column: x => x.IdObra,
                        principalTable: "Obras",
                        principalColumn: "IdObra",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeguimientoObra",
                columns: table => new
                {
                    IdSeguimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdObra = table.Column<int>(type: "int", nullable: false),
                    FechaSeguimiento = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientoObra", x => x.IdSeguimiento);
                    table.ForeignKey(
                        name: "FK_SeguimientoObra_Obra",
                        column: x => x.IdObra,
                        principalTable: "Obras",
                        principalColumn: "IdObra",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Identificador = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Clave = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol",
                        column: x => x.IdRol,
                        principalTable: "Rol",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalidaMaterial",
                columns: table => new
                {
                    IdSalida = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdBodega = table.Column<int>(type: "int", nullable: false),
                    IdInsumo = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidaMaterial", x => x.IdSalida);
                    table.ForeignKey(
                        name: "FK_SalidaMaterial_Bodega",
                        column: x => x.IdBodega,
                        principalTable: "Bodega",
                        principalColumn: "IdBodega",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalidaMaterial_Insumos",
                        column: x => x.IdInsumo,
                        principalTable: "Insumos",
                        principalColumn: "IdInsumos",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Material",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TipoUnidad = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    IdPartida = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Material_Partida",
                        column: x => x.IdPartida,
                        principalTable: "Partida",
                        principalColumn: "IdPartida");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioAcceso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    FechaHoraAcceso = table.Column<DateTime>(type: "datetime", nullable: false),
                    DireccionIP = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioAcceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioAcceso_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_IdObra",
                table: "Hitos",
                column: "IdObra");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_IdBodega",
                table: "Insumos",
                column: "IdBodega");

            migrationBuilder.CreateIndex(
                name: "IX_Material_IdPartida",
                table: "Material",
                column: "IdPartida");

            migrationBuilder.CreateIndex(
                name: "IX_Partida_IdObra",
                table: "Partida",
                column: "IdObra");

            migrationBuilder.CreateIndex(
                name: "IX_SalidaMaterial_IdBodega",
                table: "SalidaMaterial",
                column: "IdBodega");

            migrationBuilder.CreateIndex(
                name: "IX_SalidaMaterial_IdInsumo",
                table: "SalidaMaterial",
                column: "IdInsumo");

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientoObra_IdObra",
                table: "SeguimientoObra",
                column: "IdObra");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAcceso_IdUsuario",
                table: "UsuarioAcceso",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hitos");

            migrationBuilder.DropTable(
                name: "Material");

            migrationBuilder.DropTable(
                name: "SalidaMaterial");

            migrationBuilder.DropTable(
                name: "SeguimientoObra");

            migrationBuilder.DropTable(
                name: "UsuarioAcceso");

            migrationBuilder.DropTable(
                name: "Partida");

            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Obras");

            migrationBuilder.DropTable(
                name: "Bodega");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
