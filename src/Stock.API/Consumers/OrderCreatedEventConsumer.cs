using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly StockAPIDbService _stockAPIDb;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public OrderCreatedEventConsumer(StockAPIDbService stockAPIDb, ISendEndpointProvider sendEndpointProvider)
        {
            _stockAPIDb = stockAPIDb;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {

            await _semaphore.WaitAsync();

            List<int> productIds = context.Message.OrderItems.Select(x => x.ProductId).ToList();
            List<Stock.API.Entities.Stock> updatedStocks = new();

            try
            {
                var collection = _stockAPIDb.GetCollection<Stock.API.Entities.Stock>();
                var stocks = await (await collection.FindAsync(x => productIds.Contains(x.ProductId))).ToListAsync();

                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = stocks.FirstOrDefault(x => x.ProductId == orderItem.ProductId && x.Quantity >= orderItem.Count);
                    if (stock is null)
                    {
                        await SendOutOfStockEvent(context.Message.Id);
                        return;
                    }

                    stock.Quantity -= orderItem.Count;
                    updatedStocks.Add(stock);
                }

                foreach (var updatedStock in updatedStocks)
                {
                    var filter = Builders<Stock.API.Entities.Stock>.Filter.Eq(x => x.ProductId, updatedStock.ProductId);
                    var update = Builders<Stock.API.Entities.Stock>.Update.Set(x => x.Quantity, updatedStock.Quantity);
                    await collection.UpdateOneAsync(filter, update);
                }

                await SendInStockEvent(context.Message.Id, context.Message.OrderItems);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SendInStockEvent(int orderId, List<OrderItemMessage> orderItems)
        {
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(QueueNames.GetQueueUri(QueueNames.Payment_InStockEventQueue));
            await sendEndpoint.Send(new InStockEvent() { OrderId = orderId, OrderItems = orderItems });
        }

        public async Task SendOutOfStockEvent(int orderId)
        {
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(QueueNames.GetQueueUri(QueueNames.Order_OutOfStockEventQueue));
            await sendEndpoint.Send(new OutOfStockEvent() { OrderId = orderId });
        }
    }
}

