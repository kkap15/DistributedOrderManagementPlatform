using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }
    
    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected");
        return base.OnDisconnectedAsync(exception);
    }
}