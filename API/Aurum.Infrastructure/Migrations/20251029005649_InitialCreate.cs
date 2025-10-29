using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurum.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    NameCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WalletType = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    CategoryGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Transactions_Category_CategoryGuid",
                        column: x => x.CategoryGuid,
                        principalTable: "Category",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    WalletGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WalletRole = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WalletType = table.Column<string>(type: "text", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerGuide = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerUserGuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_OwnerGuid",
                        column: x => x.OwnerGuid,
                        principalTable: "Users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_OwnerUserGuid",
                        column: x => x.OwnerUserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryGuid",
                table: "Transactions",
                column: "CategoryGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedByGuid",
                table: "Transactions",
                column: "CreatedByGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletGuid",
                table: "Transactions",
                column: "WalletGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WalletGuid",
                table: "Users",
                column: "WalletGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_OwnerGuid",
                table: "Wallets",
                column: "OwnerGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_OwnerUserGuid",
                table: "Wallets",
                column: "OwnerUserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserGuid",
                table: "Wallets",
                column: "UserGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_CreatedByGuid",
                table: "Transactions",
                column: "CreatedByGuid",
                principalTable: "Users",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletGuid",
                table: "Transactions",
                column: "WalletGuid",
                principalTable: "Wallets",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Wallets_WalletGuid",
                table: "Users",
                column: "WalletGuid",
                principalTable: "Wallets",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_OwnerGuid",
                table: "Wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_OwnerUserGuid",
                table: "Wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_UserGuid",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
