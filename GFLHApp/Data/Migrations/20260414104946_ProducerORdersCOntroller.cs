using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFLHApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProducerORdersCOntroller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerOrders",
                columns: table => new
                {
                    ProducerOrdersId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdersId = table.Column<int>(type: "int", nullable: false),
                    ProducerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProducerSubtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TrackingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProducersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerOrders", x => x.ProducerOrdersId);
                    table.ForeignKey(
                        name: "FK_ProducerOrders_Orders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "Orders",
                        principalColumn: "OrdersId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProducerOrders_Producers_ProducersId",
                        column: x => x.ProducersId,
                        principalTable: "Producers",
                        principalColumn: "ProducersId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerOrders_OrdersId",
                table: "ProducerOrders",
                column: "OrdersId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerOrders_ProducersId",
                table: "ProducerOrders",
                column: "ProducersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerOrders");
        }
    }
}
