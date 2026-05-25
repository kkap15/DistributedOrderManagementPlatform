using Contracts.Messaging;
using NotificationService.Hubs;
using NotificationService.Messaging;
using NotificationService.Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IEventConsumer, OrderCreatedConsumer>();
builder.Services.AddSingleton<IEventConsumer, PaymentProcessedConsumer>();
builder.Services.AddHostedService<NotificationWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
