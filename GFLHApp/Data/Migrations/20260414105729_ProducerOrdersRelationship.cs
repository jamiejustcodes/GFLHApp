using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFLHApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProducerOrdersRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProducerOrdersId",
                table: "OrderProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_ProducerOrdersId",
                table: "OrderProducts",
                column: "ProducerOrdersId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_ProducerOrders_ProducerOrdersId",
                table: "OrderProducts",
                column: "ProducerOrdersId",
                principalTable: "ProducerOrders",
                principalColumn: "ProducerOrdersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_ProducerOrders_ProducerOrdersId",
                table: "OrderProducts");

            migrationBuilder.DropIndex(
                name: "IX_OrderProducts_ProducerOrdersId",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "ProducerOrdersId",
                table: "OrderProducts");
        }
    }
}
