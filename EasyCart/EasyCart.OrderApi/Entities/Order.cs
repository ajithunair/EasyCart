namespace EasyCart.OrderApi.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int PurchaseQuantity { get; set; }
        public int ClientId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    }
}
