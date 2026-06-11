using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class AddHoraSolicitacaoViagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSolicitacao",
                table: "Viagens",
                type: "time(0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraSolicitacao",
                table: "Viagens");
        }
    }
}
