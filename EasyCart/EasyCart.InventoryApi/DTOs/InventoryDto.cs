using System.ComponentModel.DataAnnotations;

namespace EasyCart.InventoryApi.DTOs
{
    public record InventoryDto
    (
        int Id,

        [Required] int ProductId,

        [Required, Range(0, int.MaxValue)] int AvailableQuantity,

        [Required, Range(0, int.MaxValue)] int ReorderLevel
        );

    public record StockRequest(
        [Required, Range(0, int.MaxValue)] int Quantity
        );
}
