using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFLHApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceMoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "OrderProducts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "OrderProducts");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
