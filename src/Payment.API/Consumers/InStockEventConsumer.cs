using MassTransit;
using Shared;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class InStockEventConsumer : IConsumer<InStockEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public InStockEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<InStockEvent> context)
        {
            if (context.Message.OrderId % 2 == 0)
                await _publishEndpoint.Publish(new PaymentCompletedEvent() { OrderId = context.Message.OrderId });
            else
                await _publishEndpoint.Publish(new PaymentFailedEvent() { OrderId = context.Message.OrderId, OrderItems = context.Message.OrderItems });
        }
    }
}

