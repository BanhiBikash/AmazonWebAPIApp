using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Soft_delete_mechanism_for_orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Orders",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Orders");
        }
    }
}
