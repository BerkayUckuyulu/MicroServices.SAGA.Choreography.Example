using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:UserName"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        _configure.ReceiveEndpoint(QueueNames.Stock_OrderCreatedQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(QueueNames.Stock_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<StockAPIDbService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var stockAPIDb = scope.ServiceProvider.GetRequiredService<StockAPIDbService>();
    var stockCollection = stockAPIDb.GetCollection<Stock.API.Entities.Stock>();

    await stockCollection.DeleteManyAsync(Builders<Stock.API.Entities.Stock>.Filter.Empty);

    await stockCollection.InsertOneAsync(new Stock.API.Entities.Stock { Id = 0, ProductId = 1, Quantity = 10 });
    await stockCollection.InsertOneAsync(new Stock.API.Entities.Stock { Id = 1, ProductId = 2, Quantity = 20 });
    await stockCollection.InsertOneAsync(new Stock.API.Entities.Stock { Id = 2, ProductId = 3, Quantity = 30 });
    await stockCollection.InsertOneAsync(new Stock.API.Entities.Stock { Id = 3, ProductId = 4, Quantity = 40 });
}

app.Run();
