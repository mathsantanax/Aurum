using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurum.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentNumber",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Recurrence",
                table: "FinancialTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SeriesId",
                table: "FinancialTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SeriesId",
                table: "FinancialTransactions",
                column: "SeriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_SeriesId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "InstallmentNumber",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "Recurrence",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "FinancialTransactions");
        }
    }
}
