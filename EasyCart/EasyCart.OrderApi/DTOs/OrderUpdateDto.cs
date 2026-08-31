using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record OrderUpdateDto
    (
        [Required] int Id,
        [Required, MinLength(1)] IReadOnlyCollection<OrderItemCreateDto> Items,
        [Required, Range(1, int.MaxValue)] int ClientId,
        DateTime OrderDate
    );
}
