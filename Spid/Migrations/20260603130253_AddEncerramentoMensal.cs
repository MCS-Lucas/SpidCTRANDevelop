using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spid.Migrations
{
    /// <inheritdoc />
    public partial class AddEncerramentoMensal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A tabela foi criada previamente via SQL manual.
            // Esta migration apenas sincroniza o modelo do EF Core.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncerramentosMensais");
        }
    }
}
