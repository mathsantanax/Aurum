using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurum.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletspaceFinances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastFourDigits = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    ClosingDay = table.Column<int>(type: "int", nullable: false),
                    DueDay = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditCards_Walletspaces_WalletspaceId",
                        column: x => x.WalletspaceId,
                        principalTable: "Walletspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_Walletspaces_WalletspaceId",
                        column: x => x.WalletspaceId,
                        principalTable: "Walletspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreditCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_CreditCards_CreditCardId",
                        column: x => x.CreditCardId,
                        principalTable: "CreditCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Walletspaces_WalletspaceId",
                        column: x => x.WalletspaceId,
                        principalTable: "Walletspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletspaceMembers_UserId",
                table: "WalletspaceMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_WalletspaceId",
                table: "CreditCards",
                column: "WalletspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_WalletspaceId",
                table: "FinancialAccounts",
                column: "WalletspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CreditCardId",
                table: "FinancialTransactions",
                column: "CreditCardId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FinancialAccountId",
                table: "FinancialTransactions",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_WalletspaceId_TransactionDate",
                table: "FinancialTransactions",
                columns: new[] { "WalletspaceId", "TransactionDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_WalletspaceMembers_AspNetUsers_UserId",
                table: "WalletspaceMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletspaceMembers_AspNetUsers_UserId",
                table: "WalletspaceMembers");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "CreditCards");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropIndex(
                name: "IX_WalletspaceMembers_UserId",
                table: "WalletspaceMembers");
        }
    }
}
