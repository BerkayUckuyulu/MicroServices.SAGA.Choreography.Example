using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly OrderAPIDbContext _dbContext;

        public PaymentCompletedEventConsumer(OrderAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = _dbContext.Orders.First(x => x.Id == context.Message.OrderId);
            order.Status = Constants.OrderStatus.Completed;
            await _dbContext.SaveChangesAsync();
        }
    }
}

