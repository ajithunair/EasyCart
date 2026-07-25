using System.ComponentModel.DataAnnotations;

namespace EasyCart.InventoryApi.DTOs
{
    public record InventoryCreateDto
    (
        [Required] int ProductId,
        [Required, Range(0, int.MaxValue)] int AvailableQuantity,
        [Required, Range(0, int.MaxValue)] int ReorderLevel
    );
}
