using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCart.SharedLibrary.RabbitMQ.Events
{
    public record OrderPlacedEvent
    {
        public int OrderId { get; init; }
        public DateTime OrderDate { get; init; }
        public List<OrderItemMessage> Items { get; init; } = new();
    }

    public record OrderItemMessage
    {
        public int ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
