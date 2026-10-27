using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hackatohm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertasManutencao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alertas_manutencao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MotorId = table.Column<int>(type: "integer", nullable: false),
                    SensorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NumeroEscova = table.Column<int>(type: "integer", nullable: true),
                    RegraQualidadeId = table.Column<Guid>(type: "uuid", nullable: true),
                    NomeRegra = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TipoRegra = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoRegra = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severidade = table.Column<int>(type: "integer", nullable: false),
                    ValorObservado = table.Column<double>(type: "double precision", nullable: true),
                    ValorLimite = table.Column<double>(type: "double precision", nullable: true),
                    Mensagem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IniciadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alertas_manutencao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alertas_manutencao_motores_MotorId",
                        column: x => x.MotorId,
                        principalTable: "motores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_alertas_manutencao_regras_qualidade_RegraQualidadeId",
                        column: x => x.RegraQualidadeId,
                        principalTable: "regras_qualidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_alertas_manutencao_sensores_SensorId",
                        column: x => x.SensorId,
                        principalTable: "sensores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alertas_manutencao_IniciadoEm",
                table: "alertas_manutencao",
                column: "IniciadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_manutencao_MotorId_ResolvidoEm",
                table: "alertas_manutencao",
                columns: new[] { "MotorId", "ResolvidoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_alertas_manutencao_RegraQualidadeId",
                table: "alertas_manutencao",
                column: "RegraQualidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_manutencao_SensorId",
                table: "alertas_manutencao",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alertas_manutencao");
        }
    }
}
