using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added2Columns_in_Transaction_table_to_save_payment_merchant_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMerchantOrderId",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMerchantTransactionId",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMerchantOrderId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentMerchantTransactionId",
                table: "Transactions");
        }
    }
}
