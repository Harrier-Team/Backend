using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hackatohm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorNetworkStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAtual",
                table: "sensores",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaConexao",
                table: "sensores",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAtual",
                table: "sensores");

            migrationBuilder.DropColumn(
                name: "UltimaConexao",
                table: "sensores");
        }
    }
}
