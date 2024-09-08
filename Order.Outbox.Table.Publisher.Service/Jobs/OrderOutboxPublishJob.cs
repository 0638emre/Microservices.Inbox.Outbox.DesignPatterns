using System.Text.Json;
using MassTransit;
using Order.Outbox.Table.Publisher.Service.Entities;
using Quartz;
using Shared.Events;

namespace Order.Outbox.Table.Publisher.Service.Jobs;

public class OrderOutboxPublishJob(IPublishEndpoint publishEndpoint) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (OrderOutboxSingletonDatabase.DataReaderState)
        {
            List<OrderOutbox> orderOutboxes =
                (await OrderOutboxSingletonDatabase.QueryAsync<OrderOutbox>(
                    $@"SELECT * FROM OrderOutbox WHERE ProcessedDate IS NULL ORDER BY OccuredOn DESC")).ToList();

            foreach (var orderOutbox in orderOutboxes)
            {
                if (orderOutbox.Type == nameof(OrderCreatedEvent))
                {
                    OrderCreatedEvent orderCreatedEvent =
                        JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);

                    if (orderCreatedEvent != null)
                    {
                         await publishEndpoint.Publish(orderCreatedEvent);
                         await OrderOutboxSingletonDatabase.ExecuteAsync($"UPDATE TOP(1) OrderOutbox SET ProcessedDate = GETDATE() WHERE ID = '{orderOutbox.Id}'");
                    }
                    
                }
            }
            
            OrderOutboxSingletonDatabase.DataReaderReady();
            await Console.Out.WriteLineAsync("Order outbox table checked !");
        }

        Console.WriteLine("job çlıştı");
    }
}