using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record OrderItemDetailsDto
    (
        [Required] int ProductId,
        [Required] string ProductName,
        [Required] int Quantity,
        [Required, DataType(DataType.Currency)] decimal UnitPrice,
        [Required, DataType(DataType.Currency)] decimal TotalPrice
    );

    public record OrderDetailsDto
    (
        [Required] int OrderId,
        [Required] int ClientId,
        [Required, EmailAddress] string Email,
        [Required] string Address,
        [Required] string PhoneNumber,
        [Required] IReadOnlyCollection<OrderItemDetailsDto> Items,
        [Required, DataType(DataType.Currency)] decimal TotalPrice,
        [Required] DateTime OrderDate
    );
}
