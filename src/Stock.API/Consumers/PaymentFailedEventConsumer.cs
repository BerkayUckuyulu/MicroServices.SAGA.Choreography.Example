using System;
using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly StockAPIDbService _stockAPIDb;

        public PaymentFailedEventConsumer(StockAPIDbService stockAPIDb)
        {
            _stockAPIDb = stockAPIDb;
        }

        public Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var stockCollection = _stockAPIDb.GetCollection<Stock.API.Entities.Stock>();
            context.Message.OrderItems.ForEach(async (orderItem) =>
            {
                var filterBuilder = Builders<Stock.API.Entities.Stock>.Filter.Eq(x => x.ProductId, orderItem.ProductId);
                var updateBuilder = Builders<Stock.API.Entities.Stock>.Update.Inc(x => x.Quantity, orderItem.Count);

                await stockCollection.FindOneAndUpdateAsync(filterBuilder, updateBuilder);
            });

            return Task.CompletedTask;
        }
    }
}

