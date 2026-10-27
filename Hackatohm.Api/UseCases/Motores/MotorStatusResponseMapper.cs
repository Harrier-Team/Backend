using Hackatohm.Application.MotorStatus;
using Hackatohm.Api.DTOs.MotorStatus;
using Hackatohm.Api.UseCases.Leituras;

namespace Hackatohm.Api.UseCases.Motores;

internal static class MotorStatusResponseMapper
{
    public static MotorStatusResponseDto Mapear(MotorStatusResult result) =>
        new(
            result.MotorId,
            result.SeveridadeGeral,
            result.Situacao,
            new(
                result.Contagem.Normal,
                result.Contagem.Atencao,
                result.Contagem.AlertaPreditivo,
                result.Contagem.Critico),
            result.Motivo is null
                ? null
                : new(
                    result.Motivo.NumeroEscova,
                    result.Motivo.NomeRegra,
                    result.Motivo.TipoRegra,
                    result.Motivo.CodigoRegra,
                    result.Motivo.ValorObservado,
                    result.Motivo.ValorLimite,
                    result.Motivo.MensagemDiagnostico),
            result.Escovas
                .Select(escova => new EscovaStatusResponseDto(
                    escova.SensorId,
                    escova.NumeroEscova,
                    escova.ValorCelsius,
                    escova.Severidade,
                    escova.DataHora,
                    escova.AlertaDesgaste,
                    TelemetriaRealtimeMapper.MapearResultados(escova.ResultadosQualityGates)))
                .ToArray());
}
