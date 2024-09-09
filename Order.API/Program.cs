using System.Text.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Order.API.Models.Context;
using Order.API.ViewModels;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
    });
});

builder.Services.AddDbContext<OrderAPIDBContext>(ops =>
    ops.UseSqlServer(builder.Configuration.GetConnectionString("OrderAPIDB")));

var app = builder.Build();

app.MapPost("/create-order",
    async ([FromBody] CreateOrderVM model, OrderAPIDBContext dbContext, ISendEndpointProvider sendEndpointProvider) =>
    {
        Order.API.Models.Order order = new Order.API.Models.Order()
        {
            BuyerId = model.BuyerId,
            CreatedDate = DateTime.UtcNow,
            TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
            OrderItems = model.OrderItems.Select(oi => new OrderItem
            {
                ProductId = oi.ProductId,
                Price = oi.Price,
                Count = oi.Count
            }).ToList(),
        };

        await dbContext.AddAsync(order);
        await dbContext.SaveChangesAsync();

        #region outbox patter olmaksızın klasik event publish bu şekilde idi

        // OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent()
        // {
        //     OrderId = order.Id,
        //     BuyerId = order.BuyerId,
        //     TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        //     OrderItems = order.OrderItems.Select(oi => new Shared.Datas.OrderItem()
        //     {
        //         ProductId = oi.ProductId,
        //         Count = oi.Count,
        //         Price = oi.Price
        //     }).ToList()
        // };
        //
        // var sendEndpoint =
        //     await sendEndpointProvider.GetSendEndpoint(
        //         new Uri($"queue:{RabbitMqSetting.Stock_OrderCreatedEventQueue}"));
        //
        // await sendEndpoint.Send<OrderCreatedEvent>(orderCreatedEvent);

        #endregion


        var idempotentToken = Guid.NewGuid();
        OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent()
        {
            OrderId = order.Id,
            BuyerId = order.BuyerId,
            TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
            OrderItems = order.OrderItems.Select(oi => new Shared.Datas.OrderItem()
            {
                ProductId = oi.ProductId,
                Count = oi.Count,
                Price = oi.Price
            }).ToList(),
            IdempotentToken = idempotentToken
        };
        
        OrderOutbox orderOutbox = new OrderOutbox()
        {
            OccuredOn = DateTime.UtcNow,
            ProcessedDate = null,
            Payload = JsonSerializer.Serialize(orderCreatedEvent),
            Type = orderCreatedEvent.GetType().Name, //nameof(OrderCreatedEvent)  
            IdempotentToken = idempotentToken
        };
        
        await dbContext.OrderOutbox.AddAsync(orderOutbox);
        await dbContext.SaveChangesAsync();
    });

app.UseSwagger();
app.UseSwaggerUI();

app.Run();