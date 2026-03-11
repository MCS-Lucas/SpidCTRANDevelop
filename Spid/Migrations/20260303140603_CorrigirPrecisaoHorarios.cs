using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirPrecisaoHorarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "HoraInicio",
                table: "Viagens",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "HoraFim",
                table: "Viagens",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "HoraInicio",
                table: "Viagens",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "HoraFim",
                table: "Viagens",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)");
        }
    }
}
