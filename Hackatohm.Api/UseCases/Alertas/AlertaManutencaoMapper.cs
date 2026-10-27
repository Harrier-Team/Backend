using Hackatohm.Api.DTOs;
using Hackatohm.Domain.Entities;
using Hackatohm.Domain.QualityGates;

namespace Hackatohm.Api.UseCases.Alertas;

internal static class AlertaManutencaoMapper
{
    public static AlertaManutencaoResponseDto Mapear(AlertaManutencao alerta) =>
        new(
            alerta.Id,
            alerta.MotorId,
            alerta.SensorId,
            alerta.NumeroEscova,
            alerta.RegraQualidadeId,
            alerta.NomeRegra,
            alerta.TipoRegra,
            CategoriaAlertaExtensions.DeTipoRegra(alerta.TipoRegra),
            alerta.CodigoRegra,
            alerta.Severidade,
            alerta.ValorObservado,
            alerta.ValorLimite,
            alerta.Mensagem,
            alerta.IniciadoEm,
            alerta.AtualizadoEm,
            alerta.ResolvidoEm,
            alerta.Ativo);
}
