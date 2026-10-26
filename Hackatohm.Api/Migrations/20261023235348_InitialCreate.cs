using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hackatohm.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "motores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sensores",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NumeroEscova = table.Column<int>(type: "integer", nullable: false),
                    Modelo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    MotorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sensores_motores_MotorId",
                        column: x => x.MotorId,
                        principalTable: "motores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leituras_temperatura",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SensorId = table.Column<string>(type: "character varying(64)", nullable: false),
                    ValorCelsius = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlertaDesgaste = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leituras_temperatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_leituras_temperatura_sensores_SensorId",
                        column: x => x.SensorId,
                        principalTable: "sensores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_leituras_temperatura_SensorId",
                table: "leituras_temperatura",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_sensores_MotorId",
                table: "sensores",
                column: "MotorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leituras_temperatura");

            migrationBuilder.DropTable(
                name: "sensores");

            migrationBuilder.DropTable(
                name: "motores");
        }
    }
}
