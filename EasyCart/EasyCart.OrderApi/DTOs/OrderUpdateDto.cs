using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record OrderUpdateDto
    (
        [Required] int Id,
        [Required, Range(1, int.MaxValue, ErrorMessage = "Product ID must be a positive integer")] int ProductId,
        [Required, Range(1, int.MaxValue, ErrorMessage = "Purchase Quantity must be a positive integer")] int PurchaseQuantity,
        [Required, Range(1, int.MaxValue, ErrorMessage = "Client ID must be a positive integer")] int ClientId,
        DateTime OrderDate
    );
}
