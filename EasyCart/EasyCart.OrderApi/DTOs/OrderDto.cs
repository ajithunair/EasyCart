namespace EasyCart.OrderApi.DTOs
{
    public record OrderDto
    (
        int Id,
        int ProductId,
        int PurchaseQuantity,
        int ClientId,
        DateTime OrderDate
    );
}
