using Microsoft.AspNetCore.SignalR;
using Hackatohm.Api.DTOs.MotorStatus;
using Hackatohm.Api.UseCases.Motores;

namespace Hackatohm.Api.Hubs;

public class TelemetriaHub(
    ILogger<TelemetriaHub> logger,
    ObterStatusMotorUseCase obterStatusMotorUseCase) : Hub<ITelemetriaClient>
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
        var status = await obterStatusMotorUseCase.ExecutarAsync(motorId, Context.ConnectionAborted);
        if (status is not null)
            await Clients.Caller.ReceberStatusMotor(status);
    }

    public async Task DesassinarMotor(int motorId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"motor_{motorId}");
    }
}