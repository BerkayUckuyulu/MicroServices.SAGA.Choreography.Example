using MassTransit;
using Order.API.Contexts;
using Shared.Events;

namespace Order.API.Consumers
{
    public class OutOfStockEventConsumer : IConsumer<OutOfStockEvent>
    {
        private readonly OrderAPIDbContext _dbContext;

        public OutOfStockEventConsumer(OrderAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<OutOfStockEvent> context)
        {
            var order = _dbContext.Orders.First(x => x.Id == context.Message.OrderId);
            order.Status = Constants.OrderStatus.Failed;
            _dbContext.Orders.Update(order);

            await _dbContext.SaveChangesAsync();
        }
    }
}

