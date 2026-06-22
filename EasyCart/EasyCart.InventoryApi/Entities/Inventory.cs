namespace EasyCart.InventoryApi.Entities
{
    public class Inventory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int AvailableQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

