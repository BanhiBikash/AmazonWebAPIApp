using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Removed_Items_Column_from_Transaction_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Transactions_TransactionId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_TransactionId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "OrderItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TransactionId",
                table: "OrderItems",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Transactions_TransactionId",
                table: "OrderItems",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId");
        }
    }
}
