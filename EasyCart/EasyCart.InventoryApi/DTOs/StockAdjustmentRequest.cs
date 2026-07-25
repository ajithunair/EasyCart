using System.ComponentModel.DataAnnotations;

namespace EasyCart.InventoryApi.DTOs
{
    public record StockIncreaseRequest
    (
        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")] int Quantity,
        string? Reason = null
    );

    public record StockDecreaseRequest
    (
        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")] int Quantity,
        string? Reason = null
    );
}
