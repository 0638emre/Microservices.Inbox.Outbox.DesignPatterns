using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.API.Models;
using Stock.API.Models.Context;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer(StockDbContext stockDbContext) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var existIdempotent =  await stockDbContext.OrderInboxes.AnyAsync(i => i.IdempotentToken.Equals(context.Message.IdempotentToken));

        if (!existIdempotent)
        {
            await stockDbContext.OrderInboxes.AddAsync(new()
            {
                Processed = false,
                Payload = JsonSerializer.Serialize(context.Message),
                IdempotentToken = context.Message.IdempotentToken,
            });
        
            await stockDbContext.SaveChangesAsync();
        }
        
        List<OrderInbox> orderInboxes =
            await stockDbContext.OrderInboxes.Where(i => i.Processed == false).ToListAsync();
        foreach (OrderInbox orderInbox in orderInboxes)
        {
            OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload); 
            
            await Console.Out.WriteLineAsync($"{orderCreatedEvent.OrderId} order id numarasına sahip siparişin stok işlemleri başarıyla tamamlanmıştır.");
            
            orderInbox.Processed = true;
            
            await stockDbContext.SaveChangesAsync();
        }
    }
}