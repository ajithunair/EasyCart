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

        public static Inventory ToEntity(this InventoryCreateDto inventoryDto)
        {
            return new Inventory
            {
                ProductId = inventoryDto.ProductId,
                AvailableQuantity = inventoryDto.AvailableQuantity,
                ReorderLevel = inventoryDto.ReorderLevel,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Inventory ToEntity(this InventoryUpdateDto inventoryDto)
        {
            return new Inventory
            {
                Id = inventoryDto.Id,
                ProductId = inventoryDto.ProductId,
                AvailableQuantity = inventoryDto.AvailableQuantity,
                ReorderLevel = inventoryDto.ReorderLevel,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static InventoryAdjustment ToAdjustment(this StockIncreaseRequest request)
        {
            return new InventoryAdjustment(request.Quantity, request.Reason);
        }

        public static InventoryAdjustment ToAdjustment(this StockDecreaseRequest request)
        {
            return new InventoryAdjustment(request.Quantity, request.Reason);
        }
    }

    public record InventoryAdjustment(int Quantity, string? Reason);
}
