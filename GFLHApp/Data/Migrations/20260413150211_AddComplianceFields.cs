using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFLHApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergens",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVATRegistered",
                table: "Producers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VATNumber",
                table: "Producers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAccepted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergens",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsVATRegistered",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "VATNumber",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TermsAccepted",
                table: "Orders");
        }
    }
}
