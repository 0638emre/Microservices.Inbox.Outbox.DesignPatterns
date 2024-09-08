using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Consumers;
using Stock.API.Models.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StockDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("StockAPIDB")));
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        configure.ReceiveEndpoint(RabbitMqSetting.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        
    });
});

var app = builder.Build();


app.Run();