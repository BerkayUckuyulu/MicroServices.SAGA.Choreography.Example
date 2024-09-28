using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedEvent
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

