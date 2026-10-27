using System.Buffers;
using System.Text;
using Hackatohm.Api.DTOs.Hubs;
using Hackatohm.Api.Hubs;
using Hackatohm.Domain.QualityGates.Enums;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace Hackatohm.Api.UnitTests;

public sealed class SignalRContractTests
{
    [Theory]
    [InlineData(NivelSeveridade.Normal)]
    [InlineData(NivelSeveridade.Atencao)]
    [InlineData(NivelSeveridade.AlertaPreditivo)]
    [InlineData(NivelSeveridade.Critico)]
    public void ProtocoloRegistrado_SerializaSeveridadeComoTexto(NivelSeveridade severidade)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTelemetriaSignalR(detailedErrors: false);
        using var provider = services.BuildServiceProvider();
        var protocol = provider.GetServices<IHubProtocol>().Single(item => item.Name == "json");
        var gate = new ResultadoGateRealtimeDto(
            Guid.NewGuid(), "Regra", severidade != NivelSeveridade.Normal,
            severidade, "temperatura.limiar_estatico", "TESTE", "Teste", 80, 75, null);
        var buffer = new ArrayBufferWriter<byte>();

        protocol.WriteMessage(new InvocationMessage("ReceberLoteTelemetria", [new[] { gate }]), buffer);

        var json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains($"\"severidade\":\"{severidade}\"", json);
    }
}
