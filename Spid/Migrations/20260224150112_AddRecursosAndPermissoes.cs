using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class AddRecursosAndPermissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Chave = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recursos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerfisRecurso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Perfil = table.Column<string>(type: "TEXT", nullable: false),
                    RecursoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisRecurso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfisRecurso_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_IdViagemParceiro",
                table: "Viagens",
                column: "IdViagemParceiro",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfisRecurso_Perfil_RecursoId",
                table: "PerfisRecurso",
                columns: new[] { "Perfil", "RecursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfisRecurso_RecursoId",
                table: "PerfisRecurso",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_Chave",
                table: "Recursos",
                column: "Chave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfisRecurso");

            migrationBuilder.DropTable(
                name: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Viagens_IdViagemParceiro",
                table: "Viagens");
        }
    }
}
