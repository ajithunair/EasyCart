using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record OrderItemCreateDto
    (
        [Required, Range(1, int.MaxValue)] int ProductId,
        [Required, Range(1, int.MaxValue)] int Quantity
    );

    public record OrderCreateDto
    (
        [Required, MinLength(1)] IReadOnlyCollection<OrderItemCreateDto> Items
    );
}
