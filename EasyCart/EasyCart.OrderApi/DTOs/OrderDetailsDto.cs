using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record OrderDetailsDto
    (
        [Required] int OrderId,
        [Required] int ProductId,
        [Required] int Quantity,
        [Required] int ClientId,
        [Required, EmailAddress] string Email,
        [Required, EmailAddress] string Address,
        [Required] string PhoneNumber,
        [Required] string ProductName,
        [Required] int PurchaseQuantity,
        [Required, DataType(DataType.Currency)] decimal UnitPrice,
        [Required, DataType(DataType.Currency)] decimal TotalPrice,
        [Required] DateTime OrderDate
    );
}
