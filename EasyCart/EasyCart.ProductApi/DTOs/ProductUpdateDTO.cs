using System.ComponentModel.DataAnnotations;

namespace EasyCart.ProductApi.DTOs
{
    public record ProductUpdateDTO
    (
        [Required] int Id,
        [Required] string Name,
        [Required, Range(0, int.MaxValue)] int Quantity,
        [Required, DataType(DataType.Currency)] decimal Price
    );
}
