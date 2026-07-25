namespace EasyCart.SharedLibrary.RabbitMQ.Events
{
    public record OrderPlacedEvent
    {
        public int OrderId { get; init; }
        public DateTime OrderDate { get; init; }
        public List<OrderItemMessage> Items { get; init; } = new();
    }

    public record OrderItemMessage
    {
        public int ProductId { get; init; }
        public int Quantity { get; init; }
    }

    public record InventoryReservationSucceededEvent
    {
        public int OrderId { get; init; }
    }

    public record InventoryReservationFailedEvent
    {
        public int OrderId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
