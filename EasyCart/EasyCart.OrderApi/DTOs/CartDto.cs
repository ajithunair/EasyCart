namespace EasyCart.OrderApi.DTOs
{
    public record CartItemDto(int ProductId, int Quantity);

    public record CartDto(
        int Id,
        int ClientId,
        DateTime UpdatedAt,
        IReadOnlyCollection<CartItemDto> Items);
}
