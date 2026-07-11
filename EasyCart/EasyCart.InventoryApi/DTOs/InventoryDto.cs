namespace EasyCart.InventoryApi.DTOs
{
    public record InventoryDto
    (
        int Id,
        int ProductId,
        int AvailableQuantity,
        int ReorderLevel
    );
}
