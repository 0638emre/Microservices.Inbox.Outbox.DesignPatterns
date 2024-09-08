using MassTransit;
using Order.Outbox.Table.Publisher.Service.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);
// builder.Services.AddHostedService<Worker>();

builder.Services.AddQuartz(config =>
{
    JobKey jobKey = new JobKey("OrderOutboxPublishJob");
    config.AddJob<OrderOutboxPublishJob>(opt => opt.WithIdentity(jobKey));

    TriggerKey triggerKey = new TriggerKey("OrderOutboxPublishTrigger");
    config.AddTrigger(opt => opt.ForJob(jobKey)
        .WithIdentity(triggerKey)
        .StartAt(DateTime.Now)
        .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(5).RepeatForever())
    );
});

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

builder.Services.AddQuartzHostedService(config => config.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();