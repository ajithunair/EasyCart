using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record CheckoutDto(
        [Required] string PaymentMethod,
        [Required] string ShippingAddress,
        [Required] string ShippingCity,
        [Required] string ShippingPostalCode,
        [Required] string ShippingPhone);

    public record OrderStatusUpdateDto(
        [Required] string Status,
        string? PaymentStatus,
        string? ShippingStatus,
        string? TrackingNumber);
}
