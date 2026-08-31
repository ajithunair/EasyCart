namespace EasyCart.OrderApi.DTOs
{
    public record OrderItemDto
    (
        int Id,
        int ProductId,
        int Quantity,
        decimal UnitPrice
    );

    public record OrderDto
    (
        int Id,
        int ClientId,
        DateTime OrderDate,
        string Status,
        string PaymentMethod,
        string PaymentStatus,
        string ShippingStatus,
        string ShippingAddress,
        string ShippingCity,
        string ShippingPostalCode,
        string ShippingPhone,
        string? TrackingNumber,
        IReadOnlyCollection<OrderItemDto> Items
    );
}
