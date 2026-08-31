using EasyCart.OrderApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyCart.OrderApi.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    [Migration("20260726000000_OrderLifecycle")]
    public partial class OrderLifecycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Status", table: "Orders", type: "text", nullable: false, defaultValue: "Pending");
            migrationBuilder.AddColumn<string>(name: "PaymentMethod", table: "Orders", type: "text", nullable: false, defaultValue: "CashOnDelivery");
            migrationBuilder.AddColumn<string>(name: "PaymentStatus", table: "Orders", type: "text", nullable: false, defaultValue: "Pending");
            migrationBuilder.AddColumn<string>(name: "ShippingStatus", table: "Orders", type: "text", nullable: false, defaultValue: "Pending");
            migrationBuilder.AddColumn<string>(name: "ShippingAddress", table: "Orders", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "ShippingCity", table: "Orders", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "ShippingPostalCode", table: "Orders", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "ShippingPhone", table: "Orders", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "TrackingNumber", table: "Orders", type: "text", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "ShippedAt", table: "Orders", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "DeliveredAt", table: "Orders", type: "timestamp with time zone", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Status", table: "Orders");
            migrationBuilder.DropColumn(name: "PaymentMethod", table: "Orders");
            migrationBuilder.DropColumn(name: "PaymentStatus", table: "Orders");
            migrationBuilder.DropColumn(name: "ShippingStatus", table: "Orders");
            migrationBuilder.DropColumn(name: "ShippingAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "ShippingCity", table: "Orders");
            migrationBuilder.DropColumn(name: "ShippingPostalCode", table: "Orders");
            migrationBuilder.DropColumn(name: "ShippingPhone", table: "Orders");
            migrationBuilder.DropColumn(name: "TrackingNumber", table: "Orders");
            migrationBuilder.DropColumn(name: "ShippedAt", table: "Orders");
            migrationBuilder.DropColumn(name: "DeliveredAt", table: "Orders");
        }
    }
}
