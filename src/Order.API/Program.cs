using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Contexts;
using Order.API.ViewModels;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderAPIDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OutOfStockEventConsumer>();
    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();


    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:UserName"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        _configure.ReceiveEndpoint(QueueNames.Order_OutOfStockEventQueue, e => e.ConfigureConsumer<OutOfStockEventConsumer>(context));
        _configure.ReceiveEndpoint(QueueNames.Order_PaymentCompletedEventQueue, e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
        _configure.ReceiveEndpoint(QueueNames.Order_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));

    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create", async (OrderCreateVM orderCreateVM, IPublishEndpoint publishEndpoint, OrderAPIDbContext dbContext) =>
{
    var order = new Order.API.Entities.Order()
    {
        BuyerId = orderCreateVM.BuyerId,
        Status = Order.API.Constants.OrderStatus.Suspend,
        OrderItems = orderCreateVM.OrderItems.Select(x => new Order.API.Entities.OrderItem
        {
            Count = x.Count,
            Price = x.Price,
            ProductId = x.ProductId
        }).ToList(),
        TotalPrice = orderCreateVM.OrderItems.Sum(x => x.Count * x.Price)
    };

    await dbContext.AddAsync(order);

    await dbContext.SaveChangesAsync();

    await publishEndpoint.Publish(new OrderCreatedEvent()
    {
        BuyerId = orderCreateVM.BuyerId,
        Id = order.Id,
        OrderItems = order.OrderItems.Select(x => new Shared.Messages.OrderItemMessage
        {
            Count = x.Count,
            Price = x.Price,
            ProductId = x.ProductId
        }).ToList(),
        TotalPrice = order.TotalPrice
    });

});

app.Run();


