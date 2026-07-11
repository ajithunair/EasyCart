using EasyCart.InventoryApi.Data;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;

namespace EasyCart.InventoryApi.RabbitMQ.Consumers
{
    public class OrderPlacedConsumer(InventoryDbContext dbContext) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            try
            {
                var message = context.Message;

                Console.WriteLine(Activity.Current?.TraceId);

                foreach (var item in message.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        // Ignore malformed quantities so an invalid order event can never increase stock.
                        continue;
                    }

                    // Update the stock directly in the database so concurrent consumers cannot overwrite each other.
                    var updatedRows = await dbContext.Inventories
                        .Where(i => i.ProductId == item.ProductId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(i => i.AvailableQuantity, i => i.AvailableQuantity - item.Quantity)
                            .SetProperty(i => i.UpdatedAt, DateTime.UtcNow));

                    if (updatedRows == 0)
                    {
                        Log.Warning("No inventory row found for ProductId {ProductId} while processing OrderId {OrderId}", item.ProductId, message.OrderId);
                    }
                }

                var logMessage = $"Processed Inventory for Order: {message.OrderId}";
                Console.WriteLine(logMessage);
                Log.Information(logMessage);
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
            }
        }
    }
}
