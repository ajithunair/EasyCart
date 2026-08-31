using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.Entities;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EasyCart.OrderApi.Messaging
{
    public class OrderInventoryReservationConsumer(OrderDbContext context) :
        IConsumer<InventoryReservationSucceededEvent>,
        IConsumer<InventoryReservationFailedEvent>
    {
        public async Task Consume(ConsumeContext<InventoryReservationSucceededEvent> message)
        {
            var order = await context.Orders.SingleOrDefaultAsync(order => order.Id == message.Message.OrderId);
            if (order is null) return;

            // Duplicate success events are harmless because this transition is idempotent.
            if (order.Status == OrderStatuses.Pending)
            {
                order.Status = OrderStatuses.Confirmed;
                order.ShippingStatus = ShippingStatuses.Processing;
                await context.SaveChangesAsync();
            }
        }

        public async Task Consume(ConsumeContext<InventoryReservationFailedEvent> message)
        {
            var order = await context.Orders.SingleOrDefaultAsync(order => order.Id == message.Message.OrderId);
            if (order is null) return;

            if (order.Status == OrderStatuses.Pending)
            {
                order.Status = OrderStatuses.Cancelled;
                order.ShippingStatus = ShippingStatuses.Pending;
                order.PaymentStatus = PaymentStatuses.Failed;
                await context.SaveChangesAsync();
            }

            Log.Warning("Order {OrderId} cancelled because inventory reservation failed: {Reason}",
                message.Message.OrderId, message.Message.Reason);
        }
    }
}
