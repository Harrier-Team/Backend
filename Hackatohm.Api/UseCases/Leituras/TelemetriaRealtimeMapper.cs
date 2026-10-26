using Hackatohm.Api.DTOs.Hubs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Api.UseCases.Leituras;

internal static class TelemetriaRealtimeMapper
{
    public static IReadOnlyList<ResultadoGateRealtimeDto> MapearResultados(
        IReadOnlyList<ResultadoGateQualidade> resultados) =>
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
            .ToArray();

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
            MapearResultados(resultados));
}
