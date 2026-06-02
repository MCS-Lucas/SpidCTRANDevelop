using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class RenameSetorToCentroCusto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_Setores_SetorId",
                table: "Colaboradores");

            migrationBuilder.DropForeignKey(
                name: "FK_ConferenciasMensais_Setores_SetorId",
                table: "ConferenciasMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Setores_SetorId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Viagens_Setores_SetorId",
                table: "Viagens");

            migrationBuilder.RenameTable(
                name: "Setores",
                newName: "CentrosCusto");

            migrationBuilder.RenameColumn(
                name: "SetorId",
                table: "Viagens",
                newName: "CentroCustoId");

            migrationBuilder.RenameIndex(
                name: "IX_Viagens_SetorId",
                table: "Viagens",
                newName: "IX_Viagens_CentroCustoId");

            migrationBuilder.RenameColumn(
                name: "SetorId",
                table: "Usuarios",
                newName: "CentroCustoId");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_SetorId",
                table: "Usuarios",
                newName: "IX_Usuarios_CentroCustoId");

            migrationBuilder.RenameColumn(
                name: "SetorId",
                table: "ConferenciasMensais",
                newName: "CentroCustoId");

            migrationBuilder.RenameIndex(
                name: "IX_ConferenciasMensais_SetorId_Ano_Mes",
                table: "ConferenciasMensais",
                newName: "IX_ConferenciasMensais_CentroCustoId_Ano_Mes");

            migrationBuilder.RenameColumn(
                name: "SetorId",
                table: "Colaboradores",
                newName: "CentroCustoId");

            migrationBuilder.RenameIndex(
                name: "IX_Colaboradores_SetorId",
                table: "Colaboradores",
                newName: "IX_Colaboradores_CentroCustoId");



            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_CentrosCusto_CentroCustoId",
                table: "Colaboradores",
                column: "CentroCustoId",
                principalTable: "CentrosCusto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConferenciasMensais_CentrosCusto_CentroCustoId",
                table: "ConferenciasMensais",
                column: "CentroCustoId",
                principalTable: "CentrosCusto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_CentrosCusto_CentroCustoId",
                table: "Usuarios",
                column: "CentroCustoId",
                principalTable: "CentrosCusto",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Viagens_CentrosCusto_CentroCustoId",
                table: "Viagens",
                column: "CentroCustoId",
                principalTable: "CentrosCusto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_CentrosCusto_CentroCustoId",
                table: "Colaboradores");

            migrationBuilder.DropForeignKey(
                name: "FK_ConferenciasMensais_CentrosCusto_CentroCustoId",
                table: "ConferenciasMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_CentrosCusto_CentroCustoId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Viagens_CentrosCusto_CentroCustoId",
                table: "Viagens");

            migrationBuilder.RenameTable(
                name: "CentrosCusto",
                newName: "Setores");

            migrationBuilder.RenameColumn(
                name: "CentroCustoId",
                table: "Viagens",
                newName: "SetorId");

            migrationBuilder.RenameIndex(
                name: "IX_Viagens_CentroCustoId",
                table: "Viagens",
                newName: "IX_Viagens_SetorId");

            migrationBuilder.RenameColumn(
                name: "CentroCustoId",
                table: "Usuarios",
                newName: "SetorId");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_CentroCustoId",
                table: "Usuarios",
                newName: "IX_Usuarios_SetorId");

            migrationBuilder.RenameColumn(
                name: "CentroCustoId",
                table: "ConferenciasMensais",
                newName: "SetorId");

            migrationBuilder.RenameIndex(
                name: "IX_ConferenciasMensais_CentroCustoId_Ano_Mes",
                table: "ConferenciasMensais",
                newName: "IX_ConferenciasMensais_SetorId_Ano_Mes");

            migrationBuilder.RenameColumn(
                name: "CentroCustoId",
                table: "Colaboradores",
                newName: "SetorId");

            migrationBuilder.RenameIndex(
                name: "IX_Colaboradores_CentroCustoId",
                table: "Colaboradores",
                newName: "IX_Colaboradores_SetorId");



            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_Setores_SetorId",
                table: "Colaboradores",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConferenciasMensais_Setores_SetorId",
                table: "ConferenciasMensais",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Setores_SetorId",
                table: "Usuarios",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Viagens_Setores_SetorId",
                table: "Viagens",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
