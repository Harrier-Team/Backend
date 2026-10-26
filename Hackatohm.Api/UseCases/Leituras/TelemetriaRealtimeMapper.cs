using Hackatohm.Api.DTOs.Hubs;
using Hackatohm.Api.Entities;
using Hackatohm.Api.QualityGates.Models;

namespace Hackatohm.Api.UseCases.Leituras;

internal static class TelemetriaRealtimeMapper
{
    public static TelemetriaRealtimeDto Mapear(
        LeituraTemperatura leitura,
        string espMacAddress,
        Sensor sensor,
        IReadOnlyList<ResultadoGateQualidade> resultados) =>
        new(
            leitura.SensorId,
            espMacAddress,
            sensor.NumeroEscova,
            leitura.ValorCelsius,
            leitura.DataHora,
            leitura.AlertaDesgaste,
            sensor.MotorId,
            resultados
                .Select(resultado => new ResultadoGateRealtimeDto(
                    resultado.RegraQualidadeId!.Value,
                    resultado.NomeRegra!,
                    resultado.DisparouAlerta,
                    resultado.Severidade,
                    resultado.TipoRegra,
                    resultado.CodigoRegra,
                    resultado.MensagemDiagnostico,
                    resultado.ValorObservado,
                    resultado.ValorLimite,
                    resultado.Metadados))
                .ToArray());
}
