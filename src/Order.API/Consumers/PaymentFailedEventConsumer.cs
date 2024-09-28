using MassTransit;
using Order.API.Contexts;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly OrderAPIDbContext _dbContext;

        public PaymentFailedEventConsumer(OrderAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = _dbContext.Orders.First(x => x.Id == context.Message.OrderId);
            order.Status = Constants.OrderStatus.Completed;
            await _dbContext.SaveChangesAsync();
        }
    }
}

