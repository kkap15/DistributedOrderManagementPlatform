using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;
using OrderService.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Contracts;
using Contracts.Messaging;
using Infrastructure.Extensions;
using OrderService.Messaging;
using OrderService.Worker;

namespace OrderService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        var dbPath = builder.Configuration.GetConnectionString("OrderDb");
        if (dbPath?.Contains("Data Source=") == true)
        {
            var path = dbPath.Replace("Data Source=", "").Split('.')[0];
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        builder.Services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("OrderDb")));
        
        builder.Services.AddScoped<IOrderRepositories, OrderRepositories>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddKafkaPublisher();
        builder.Services.AddSingleton<IEventConsumer, PaymentProcessedConsumer>();
        builder.Services.AddHostedService<OrderConsumerWorker>();
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" });
        });

        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<OrderDbContext>().Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1"));
        }
        
        app.MapControllers();

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        };
        
        using var adminClient = new AdminClientBuilder(adminConfig).Build();
        try
        {
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification { Name = Topics.OrderCreated, NumPartitions = 1, ReplicationFactor = 1 },
                new TopicSpecification { Name = Topics.PaymentProcessed, NumPartitions = 1, ReplicationFactor = 1 },
            });
            Console.WriteLine("Kafka topics created successfully");
        }
        catch (CreateTopicsException e) when (e.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            Console.WriteLine("Kafka topic already exists");
        }
        
        await app.RunAsync();

    }
}
