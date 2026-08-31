namespace EasyCart.OrderApi.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
