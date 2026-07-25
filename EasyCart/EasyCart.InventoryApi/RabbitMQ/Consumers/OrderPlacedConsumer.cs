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
    public class OrderPlacedConsumer(InventoryDbContext dbContext) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;
            Console.WriteLine(Activity.Current?.TraceId);

            // Aggregate duplicate product lines first so one product is reserved exactly once per order.
            var requestedItems = message.Items
                .Where(item => item.ProductId > 0 && item.Quantity > 0)
                .GroupBy(item => item.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                })
                // Use a stable lock order to reduce deadlock risk when concurrent orders share products.
                .OrderBy(item => item.ProductId)
                .ToList();

            if (requestedItems.Count == 0)
                throw new InvalidOperationException($"Order {message.OrderId} contains no valid inventory items.");

            try
            {
                // Npgsql's retrying strategy requires the complete user transaction to run inside ExecuteAsync.
                var executionStrategy = dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                    foreach (var item in requestedItems)
                    {
                        // The quantity predicate makes the decrement concurrency-safe: zero affected rows means
                        // the row is missing or another order consumed the remaining stock first.
                        var updatedRows = await dbContext.Inventories
                            .Where(inventory => inventory.ProductId == item.ProductId &&
                                                inventory.AvailableQuantity >= item.Quantity)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(inventory => inventory.AvailableQuantity,
                                    inventory => inventory.AvailableQuantity - item.Quantity)
                                .SetProperty(inventory => inventory.UpdatedAt, DateTime.UtcNow));

                        if (updatedRows != 1)
                        {
                            throw new InvalidOperationException(
                                $"Insufficient or missing inventory for ProductId {item.ProductId} in Order {message.OrderId}.");
                        }
                    }

                    // Commit only after every item has reserved successfully; any failure rolls back all decrements.
                    await transaction.CommitAsync();
                });

                Log.Information("Reserved inventory for Order {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                // Re-throw so MassTransit marks the message as failed instead of acknowledging a partial reservation.
                throw;
            }
        }
    }
}
