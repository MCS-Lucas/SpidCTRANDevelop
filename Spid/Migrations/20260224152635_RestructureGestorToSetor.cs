using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class RestructureGestorToSetor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Viagens_Usuarios_GestorResponsavelId",
                table: "Viagens");

            migrationBuilder.DropIndex(
                name: "IX_Viagens_GestorResponsavelId",
                table: "Viagens");

            migrationBuilder.DropColumn(
                name: "GestorResponsavelId",
                table: "Viagens");

            migrationBuilder.AddColumn<int>(
                name: "SetorId",
                table: "Usuarios",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_SetorId",
                table: "Usuarios",
                column: "SetorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Setores_SetorId",
                table: "Usuarios",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Setores_SetorId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_SetorId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SetorId",
                table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "GestorResponsavelId",
                table: "Viagens",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_GestorResponsavelId",
                table: "Viagens",
                column: "GestorResponsavelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Viagens_Usuarios_GestorResponsavelId",
                table: "Viagens",
                column: "GestorResponsavelId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
