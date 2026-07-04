using EasyCart.InventoryApi.Entities;

namespace EasyCart.InventoryApi.DTOs.Conversions
{
    public static class InventoryConversion
    {
        public static InventoryDto ToDto(this Inventory inventory)
        {
            return new InventoryDto(inventory.Id, inventory.ProductId, inventory.AvailableQuantity, inventory.ReorderLevel);
        }

        public static Inventory ToEntity(this InventoryDto inventoryDto)
        {
            return new Inventory
            {
                Id = inventoryDto.Id,
                ProductId = inventoryDto.ProductId,
                AvailableQuantity = inventoryDto.AvailableQuantity,
                ReorderLevel = inventoryDto.ReorderLevel,
                UpdatedAt= DateTime.UtcNow
            };
        }
    }
}
