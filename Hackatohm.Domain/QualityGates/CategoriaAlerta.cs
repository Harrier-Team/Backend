using Hackatohm.Domain.QualityGates.Constants;

namespace Hackatohm.Domain.QualityGates;

/// <summary>Agrupa os tipos de regra pelo que o alerta comunica ao técnico.</summary>
public enum CategoriaAlerta
{
    /// <summary>O que vai acontecer: estimativa de quando um limite será atingido.</summary>
    Preditivo,

    /// <summary>A rapidez com que a temperatura está subindo.</summary>
    Aceleracao,

    /// <summary>O que está acontecendo agora: limite excedido ou escovas divergentes.</summary>
    Condicao
}

public static class CategoriaAlertaExtensions
{
    private static readonly string[] TiposConhecidos =
    [
        TiposRegraTemperatura.LimiarEstatico,
        TiposRegraTemperatura.GradienteTermico,
        TiposRegraTemperatura.AssimetriaEscovas,
        TiposRegraTemperatura.PrevisaoTendencia,
        TiposRegraTemperatura.PrevisaoDesgasteMl
    ];

    public static CategoriaAlerta DeTipoRegra(string tipoRegra) =>
        tipoRegra switch
        {
            TiposRegraTemperatura.PrevisaoTendencia or TiposRegraTemperatura.PrevisaoDesgasteMl =>
                CategoriaAlerta.Preditivo,
            TiposRegraTemperatura.GradienteTermico => CategoriaAlerta.Aceleracao,
            _ => CategoriaAlerta.Condicao
        };

    public static IReadOnlyList<string> TiposRegra(this CategoriaAlerta categoria) =>
        TiposConhecidos.Where(tipo => DeTipoRegra(tipo) == categoria).ToArray();
}
