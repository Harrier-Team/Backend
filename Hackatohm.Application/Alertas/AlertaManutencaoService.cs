using Hackatohm.Application.Abstractions.Persistence;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;
using Hackatohm.Domain.QualityGates.Constants;

namespace Hackatohm.Application.Alertas;

public sealed class AlertaManutencaoService(
    IAlertaManutencaoRepository alertaRepository) : IAlertaManutencaoService
{
    private const int TamanhoMaximoMensagem = 500;

    private readonly record struct Chave(Guid RegraId, string? SensorId);

    public async Task<IReadOnlyList<AlertaManutencao>> RegistrarAsync(
        int motorId,
        IReadOnlyList<AvaliacaoSensor> avaliacoes,
        DateTimeOffset agora,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(avaliacoes);

        var candidatos = new Dictionary<Chave, (AvaliacaoSensor Avaliacao, ResultadoGateQualidade Resultado)>();
        foreach (var avaliacao in avaliacoes.OrderBy(item => item.Sensor.NumeroEscova))
        {
            foreach (var resultado in avaliacao.Resultados)
            {
                if (!resultado.DisparouAlerta || resultado.RegraQualidadeId is not { } regraId)
                    continue;

                // A assimetria descreve o motor inteiro: todas as escovas recebem o mesmo resultado.
                var porMotor = resultado.TipoRegra == TiposRegraTemperatura.AssimetriaEscovas;
                candidatos.TryAdd(
                    new Chave(regraId, porMotor ? null : avaliacao.Sensor.Id),
                    (avaliacao, resultado));
            }
        }

        var abertos = await alertaRepository.ObterAbertosPorMotorAsync(motorId, cancellationToken);
        var notificar = new List<AlertaManutencao>();

        foreach (var aberto in abertos)
        {
            var chave = new Chave(aberto.RegraQualidadeId ?? Guid.Empty, aberto.SensorId);
            if (aberto.RegraQualidadeId is not null && candidatos.Remove(chave, out var candidato))
            {
                var subiu = candidato.Resultado.Severidade > aberto.Severidade;
                Preencher(aberto, candidato.Resultado);
                aberto.AtualizadoEm = agora;
                if (subiu)
                    notificar.Add(aberto);
            }
            else
            {
                aberto.ResolvidoEm = agora;
                aberto.AtualizadoEm = agora;
            }
        }

        var novos = candidatos
            .Select(item =>
            {
                var (avaliacao, resultado) = item.Value;
                var porMotor = item.Key.SensorId is null;
                var alerta = new AlertaManutencao
                {
                    MotorId = motorId,
                    SensorId = porMotor ? null : avaliacao.Sensor.Id,
                    NumeroEscova = porMotor ? null : avaliacao.Sensor.NumeroEscova,
                    RegraQualidadeId = item.Key.RegraId,
                    IniciadoEm = agora,
                    AtualizadoEm = agora
                };
                Preencher(alerta, resultado);
                return alerta;
            })
            .ToArray();

        if (novos.Length > 0)
            await alertaRepository.AdicionarRangeAsync(novos, cancellationToken);

        return [.. novos, .. notificar];
    }

    private static void Preencher(AlertaManutencao alerta, ResultadoGateQualidade resultado)
    {
        alerta.NomeRegra = resultado.NomeRegra ?? resultado.TipoRegra;
        alerta.TipoRegra = resultado.TipoRegra;
        alerta.CodigoRegra = resultado.CodigoRegra;
        alerta.Severidade = resultado.Severidade;
        alerta.ValorObservado = resultado.ValorObservado;
        alerta.ValorLimite = resultado.ValorLimite;
        alerta.Mensagem = resultado.MensagemDiagnostico.Length > TamanhoMaximoMensagem
            ? resultado.MensagemDiagnostico[..TamanhoMaximoMensagem]
            : resultado.MensagemDiagnostico;
    }
}
