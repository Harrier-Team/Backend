using Microsoft.AspNetCore.SignalR;

namespace Hackatohm.Api.Hubs;

public class TelemetriaHub(ILogger<TelemetriaHub> logger) : Hub<ITelemetriaClient>
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Cliente conectado ao Hub: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    // Permite que um cliente assine apenas um motor específico se desejar (ex: tela de detalhes)
    public async Task AssinarMotor(int motorId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"motor_{motorId}");
    }

    public async Task DesassinarMotor(int motorId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"motor_{motorId}");
    }
}