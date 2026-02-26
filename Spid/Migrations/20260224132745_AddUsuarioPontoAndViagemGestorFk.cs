using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioPontoAndViagemGestorFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ponto",
                table: "Usuarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_GestorResponsavelId",
                table: "Viagens",
                column: "GestorResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Ponto",
                table: "Usuarios",
                column: "Ponto",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Viagens_Usuarios_GestorResponsavelId",
                table: "Viagens",
                column: "GestorResponsavelId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Viagens_Usuarios_GestorResponsavelId",
                table: "Viagens");

            migrationBuilder.DropIndex(
                name: "IX_Viagens_GestorResponsavelId",
                table: "Viagens");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Ponto",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Ponto",
                table: "Usuarios");
        }
    }
}
