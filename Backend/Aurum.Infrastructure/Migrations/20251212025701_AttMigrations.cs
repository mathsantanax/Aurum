using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurum.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. RENOMEAR a coluna de IsIncome para Flow, para refletir o nome da propriedade no C#
            migrationBuilder.RenameColumn(
                name: "IsIncome",
                table: "Categories",
                newName: "Flow");

            // 2. ALTERAR o tipo da coluna de boolean para TEXT (VARCHAR)
            // Tornamos a coluna temporariamente nullable para permitir a migração de dados
            migrationBuilder.AlterColumn<string>(
                name: "Flow",
                table: "Categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            // 3. LÓGICA DE MIGRAÇÃO DE DADOS (SQL)
            // O PostgreSQL converte boolean TRUE para 1 e FALSE para 0 se for forçado para int (::int).
            // Mapeamento: TRUE ('Income') -> 'Income' | FALSE (não 'Income') -> 'Expense'
            var sqlMigration = $@"
                UPDATE ""Categories""
                SET ""Flow"" = CASE
                    -- TRUE (1) era 'IsIncome'
                    WHEN ""Flow""::int = 1 THEN 'Income' 
                    -- FALSE (0) não era 'IsIncome', portanto era uma Despesa
                    WHEN ""Flow""::int = 0 THEN 'Expense'
                    ELSE 'Expense' -- Fallback
                END;
            ";

            migrationBuilder.Sql(sqlMigration);

            // 4. TORNAR a nova coluna NOT NULL novamente, após a migração de dados
            migrationBuilder.AlterColumn<string>(
                name: "Flow",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "Expense", // Define um valor padrão para novas categorias
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lógica para reverter a migração

            // 1. Alterar o tipo da coluna de TEXT para boolean (temporariamente nullable)
            migrationBuilder.AlterColumn<bool>(
                name: "Flow",
                table: "Categories",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            // 2. Reverter os dados para o booleano (Income = TRUE, Expense/Transfer = FALSE)
            var sqlRevert = $@"
                UPDATE ""Categories""
                SET ""Flow"" = CASE
                    WHEN ""Flow"" = 'Income' THEN TRUE
                    ELSE FALSE
                END;
            ";
            migrationBuilder.Sql(sqlRevert);

            // 3. Tornar a coluna NOT NULL novamente
            migrationBuilder.AlterColumn<bool>(
                name: "Flow",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            // 4. RENOMEAR a coluna de volta para IsIncome
            migrationBuilder.RenameColumn(
                name: "Flow",
                table: "Categories",
                newName: "IsIncome");
        }
    }
}
