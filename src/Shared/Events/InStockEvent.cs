using Shared.Messages;

namespace Shared.Events
{
    public class InStockEvent
    {
        public int OrderId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}