using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record ProductDto
    (
        int Id,
        [Required] string Name,
        [Required, Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer")] int Quantity,
        [Required, DataType(DataType.Currency, ErrorMessage = "Price must be a valid currency amount")] decimal Price
        );
}
