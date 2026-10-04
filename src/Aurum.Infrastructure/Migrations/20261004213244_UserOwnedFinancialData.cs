using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurum.Infrastructure.Migrations;

public partial class UserOwnedFinancialData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM FinancialAccounts account
                LEFT JOIN AspNetUsers owner ON owner.Id = account.CreatedBy
                WHERE owner.Id IS NULL
            )
                THROW 51000, 'Cannot migrate financial accounts: CreatedBy does not identify an existing user.', 1;

            IF EXISTS (
                SELECT 1
                FROM CreditCards card
                LEFT JOIN AspNetUsers owner ON owner.Id = card.CreatedBy
                WHERE owner.Id IS NULL
            )
                THROW 51000, 'Cannot migrate credit cards: CreatedBy does not identify an existing user.', 1;

            IF EXISTS (
                SELECT 1
                FROM FinancialTransactions tx
                WHERE (tx.FinancialAccountId IS NULL AND tx.CreditCardId IS NULL)
                   OR (tx.FinancialAccountId IS NOT NULL AND tx.CreditCardId IS NOT NULL)
            )
                THROW 51000, 'Cannot migrate transactions: each transaction must reference exactly one account or card.', 1;

            IF EXISTS (
                SELECT 1
                FROM FinancialTransactions tx
                LEFT JOIN AspNetUsers actor ON actor.Id = tx.CreatedBy
                WHERE actor.Id IS NULL
            )
                THROW 51000, 'Cannot migrate transactions: CreatedBy does not identify an existing user for share audit.', 1;

            IF EXISTS (
                SELECT 1
                FROM FinancialTransactions tx
                LEFT JOIN FinancialAccounts account ON account.Id = tx.FinancialAccountId
                LEFT JOIN CreditCards card ON card.Id = tx.CreditCardId
                LEFT JOIN AspNetUsers owner ON owner.Id = COALESCE(account.CreatedBy, card.CreatedBy)
                WHERE owner.Id IS NULL
            )
                THROW 51000, 'Cannot migrate transactions: the referenced account or card has no valid owner.', 1;
            """);

        migrationBuilder.DropForeignKey(
            name: "FK_CreditCards_Walletspaces_WalletspaceId",
            table: "CreditCards");
        migrationBuilder.DropForeignKey(
            name: "FK_FinancialAccounts_Walletspaces_WalletspaceId",
            table: "FinancialAccounts");
        migrationBuilder.DropForeignKey(
            name: "FK_FinancialTransactions_Walletspaces_WalletspaceId",
            table: "FinancialTransactions");

        migrationBuilder.CreateTable(
            name: "TransactionWalletspaceShares",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FinancialTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WalletspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TransactionWalletspaceShares", share => share.Id);
                table.ForeignKey(
                    name: "FK_TransactionWalletspaceShares_FinancialTransactions_FinancialTransactionId",
                    column: share => share.FinancialTransactionId,
                    principalTable: "FinancialTransactions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TransactionWalletspaceShares_Walletspaces_WalletspaceId",
                    column: share => share.WalletspaceId,
                    principalTable: "Walletspaces",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TransactionWalletspaceShares_FinancialTransactionId_WalletspaceId",
            table: "TransactionWalletspaceShares",
            columns: new[] { "FinancialTransactionId", "WalletspaceId" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_TransactionWalletspaceShares_WalletspaceId",
            table: "TransactionWalletspaceShares",
            column: "WalletspaceId");

        migrationBuilder.Sql(
            """
            INSERT INTO TransactionWalletspaceShares
                (Id, FinancialTransactionId, WalletspaceId, CreatedAt, CreatedBy)
            SELECT NEWID(), Id, WalletspaceId, CreatedAt, CreatedBy
            FROM FinancialTransactions;

            UPDATE FinancialAccounts SET WalletspaceId = CreatedBy;
            UPDATE CreditCards SET WalletspaceId = CreatedBy;

            UPDATE tx
            SET WalletspaceId = COALESCE(account.CreatedBy, card.CreatedBy)
            FROM FinancialTransactions tx
            LEFT JOIN FinancialAccounts account ON account.Id = tx.FinancialAccountId
            LEFT JOIN CreditCards card ON card.Id = tx.CreditCardId;
            """);

        migrationBuilder.RenameColumn(
            name: "WalletspaceId",
            table: "FinancialTransactions",
            newName: "OwnerUserId");
        migrationBuilder.RenameIndex(
            name: "IX_FinancialTransactions_WalletspaceId_TransactionDate",
            table: "FinancialTransactions",
            newName: "IX_FinancialTransactions_OwnerUserId_TransactionDate");
        migrationBuilder.RenameColumn(
            name: "WalletspaceId",
            table: "FinancialAccounts",
            newName: "OwnerUserId");
        migrationBuilder.RenameIndex(
            name: "IX_FinancialAccounts_WalletspaceId",
            table: "FinancialAccounts",
            newName: "IX_FinancialAccounts_OwnerUserId");
        migrationBuilder.RenameColumn(
            name: "WalletspaceId",
            table: "CreditCards",
            newName: "OwnerUserId");
        migrationBuilder.RenameIndex(
            name: "IX_CreditCards_WalletspaceId",
            table: "CreditCards",
            newName: "IX_CreditCards_OwnerUserId");

        migrationBuilder.AddCheckConstraint(
            name: "CK_FinancialTransactions_ExactlyOneResource",
            table: "FinancialTransactions",
            sql: "([FinancialAccountId] IS NOT NULL AND [CreditCardId] IS NULL) OR ([FinancialAccountId] IS NULL AND [CreditCardId] IS NOT NULL)");

        migrationBuilder.AddForeignKey(
            name: "FK_CreditCards_AspNetUsers_OwnerUserId",
            table: "CreditCards",
            column: "OwnerUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(
            name: "FK_FinancialAccounts_AspNetUsers_OwnerUserId",
            table: "FinancialAccounts",
            column: "OwnerUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(
            name: "FK_FinancialTransactions_AspNetUsers_OwnerUserId",
            table: "FinancialTransactions",
            column: "OwnerUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException(
            "This migration cannot be safely reverted: personal ownership and multiple Walletspace shares cannot be represented by the previous single-Walletspace schema.");
}
