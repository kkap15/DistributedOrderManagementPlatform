using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OrderService.Data;
using OrderService.Services;
using Microsoft.EntityFrameworkCore;
using OrderService.Repositories;
using Microsoft.Extensions.Configuration;
using System;

namespace OrderService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("OrderDb")));
        builder.Services.AddScoped<IOrderRepositories, OrderRepositories>();
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddHttpClient<PaymentClient>();
        builder.Services.AddScoped<PaymentClient>();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" });
        });
        builder.Services.AddHttpClient<PaymentClient>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["PaymentService:BaseUrl"] ?? "http://localhost:5001"
            );
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

        app.Run();

    }
}
