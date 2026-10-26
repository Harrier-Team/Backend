using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hackatohm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityRulesAndMotorLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regras_qualidade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TipoRegra = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SeveridadePadrao = table.Column<int>(type: "integer", nullable: false),
                    ParametrosJson = table.Column<string>(type: "jsonb", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regras_qualidade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "motores_regras_qualidade",
                columns: table => new
                {
                    MotorId = table.Column<int>(type: "integer", nullable: false),
                    RegraQualidadeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    VinculadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motores_regras_qualidade", x => new { x.MotorId, x.RegraQualidadeId });
                    table.ForeignKey(
                        name: "FK_motores_regras_qualidade_motores_MotorId",
                        column: x => x.MotorId,
                        principalTable: "motores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_motores_regras_qualidade_regras_qualidade_RegraQualidadeId",
                        column: x => x.RegraQualidadeId,
                        principalTable: "regras_qualidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_motores_regras_qualidade_MotorId_Ativo",
                table: "motores_regras_qualidade",
                columns: new[] { "MotorId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_motores_regras_qualidade_RegraQualidadeId",
                table: "motores_regras_qualidade",
                column: "RegraQualidadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "motores_regras_qualidade");

            migrationBuilder.DropTable(
                name: "regras_qualidade");
        }
    }
}
