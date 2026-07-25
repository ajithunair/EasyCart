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
        IReadOnlyCollection<OrderItemDto> Items
    );
}
