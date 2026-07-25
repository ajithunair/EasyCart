namespace EasyCart.OrderApi.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Store the price used at checkout so historical orders do not change when catalog prices change.
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; } = null!;
    }
}
