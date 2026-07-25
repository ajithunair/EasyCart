namespace EasyCart.OrderApi.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // An order is the aggregate root; all purchased products belong in its item collection.
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
