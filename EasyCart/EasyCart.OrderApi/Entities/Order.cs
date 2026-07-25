namespace EasyCart.OrderApi.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = OrderStatuses.Pending;
        public string PaymentMethod { get; set; } = "CashOnDelivery";
        public string PaymentStatus { get; set; } = PaymentStatuses.Pending;
        public string ShippingStatus { get; set; } = ShippingStatuses.Pending;

        // Store the delivery address on the order so later profile edits do not change historical shipments.
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // An order is the aggregate root; all purchased products belong in its item collection.
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
