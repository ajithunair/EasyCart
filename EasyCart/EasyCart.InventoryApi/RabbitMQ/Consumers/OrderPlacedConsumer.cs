using EasyCart.InventoryApi.Data;
using EasyCart.InventoryApi.Interfaces;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EasyCart.InventoryApi.RabbitMQ.Consumers
{
    public class OrderPlacedConsumer(InventoryDbContext dbContext) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            try
            {
                var message = context.Message;

                foreach (var item in message.Items)
                {
                    var inventory = await dbContext.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                    if (inventory != null)
                    {
                        inventory.AvailableQuantity -= item.Quantity;
                        inventory.UpdatedAt = DateTime.UtcNow;
                    }

                }
                await dbContext.SaveChangesAsync();
                var logMessage = $"Processed Inventory for Order: {message.OrderId}";
                Console.WriteLine(logMessage);
                Serilog.Log.Information(logMessage);
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
            }
        }
    }
}
