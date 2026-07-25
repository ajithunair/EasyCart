using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyCart.OrderApi.Migrations
{
    /// <inheritdoc />
    [Migration("20260725170000_OrderItems")]
    public partial class OrderItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Preserve existing single-product orders while moving them into the new child table.
            migrationBuilder.Sql("""
                INSERT INTO "OrderItems" ("OrderId", "ProductId", "Quantity", "UnitPrice")
                SELECT "Id", "ProductId", "PurchaseQuantity", 0
                FROM "Orders";
                """);

            migrationBuilder.DropColumn(name: "ProductId", table: "Orders");
            migrationBuilder.DropColumn(name: "PurchaseQuantity", table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "ProductId", table: "Orders", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "PurchaseQuantity", table: "Orders", type: "integer", nullable: false, defaultValue: 0);

            // Restore the first item for rollback compatibility with the original one-product schema.
            migrationBuilder.Sql("""
                UPDATE "Orders" AS orders
                SET "ProductId" = items."ProductId",
                    "PurchaseQuantity" = items."Quantity"
                FROM "OrderItems" AS items
                WHERE items."OrderId" = orders."Id";
                """);

            migrationBuilder.DropTable(name: "OrderItems");
        }
    }
}
