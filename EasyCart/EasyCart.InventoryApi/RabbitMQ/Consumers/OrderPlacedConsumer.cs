using EasyCart.InventoryApi.Data;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;
using System.Diagnostics;

namespace EasyCart.InventoryApi.RabbitMQ.Consumers
{
    public class OrderPlacedConsumer(InventoryDbContext dbContext, IPublishEndpoint publishEndpoint) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;
            Console.WriteLine(Activity.Current?.TraceId);

            var requestedItems = message.Items
                .Where(item => item.ProductId > 0 && item.Quantity > 0)
                .GroupBy(item => item.ProductId)
                .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
                .OrderBy(item => item.ProductId)
                .ToList();

            if (requestedItems.Count == 0)
                throw new InvalidOperationException($"Order {message.OrderId} contains no valid inventory items.");

            try
            {
                var executionStrategy = dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                    foreach (var item in requestedItems)
                    {
                        // Conditional update prevents negative stock and makes concurrent reservations safe.
                        var updatedRows = await dbContext.Inventories
                            .Where(inventory => inventory.ProductId == item.ProductId &&
                                                inventory.AvailableQuantity >= item.Quantity)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(inventory => inventory.AvailableQuantity,
                                    inventory => inventory.AvailableQuantity - item.Quantity)
                                .SetProperty(inventory => inventory.UpdatedAt, DateTime.UtcNow));

                        if (updatedRows != 1)
                            throw new InvalidOperationException($"Insufficient or missing inventory for ProductId {item.ProductId} in Order {message.OrderId}.");
                    }

                    // Commit only after every line succeeds, guaranteeing all-or-nothing reservation.
                    await transaction.CommitAsync();
                });

                Log.Information("Reserved inventory for Order {OrderId}", message.OrderId);
                await publishEndpoint.Publish(new InventoryReservationSucceededEvent { OrderId = message.OrderId });
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                await publishEndpoint.Publish(new InventoryReservationFailedEvent
                {
                    OrderId = message.OrderId,
                    Reason = ex.Message
                });
                // Preserve the failure so MassTransit does not acknowledge a failed reservation.
                throw;
            }
        }
    }
}
