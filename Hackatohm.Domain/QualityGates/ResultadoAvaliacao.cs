using Hackatohm.Domain.QualityGates.Enums;

namespace Hackatohm.Domain.QualityGates;

/// <summary>
/// Representa o resultado estruturado da avaliação de um Quality Gate sobre uma leitura de telemetria.
/// </summary>
public sealed record ResultadoGateQualidade
{
    private ResultadoGateQualidade(
        bool disparouAlerta,
        NivelSeveridade severidade,
        string tipoRegra,
        string codigoRegra,
        string mensagemDiagnostico,
        double? valorObservado,
        double? valorLimite,
        IReadOnlyDictionary<string, object>? metadados)
    {
        DisparouAlerta = disparouAlerta;
        Severidade = severidade;
        TipoRegra = tipoRegra;
        CodigoRegra = codigoRegra;
        MensagemDiagnostico = mensagemDiagnostico;
        ValorObservado = valorObservado;
        ValorLimite = valorLimite;
        Metadados = metadados;
    }

    public bool DisparouAlerta { get; }
    public NivelSeveridade Severidade { get; }
    public string TipoRegra { get; }
    public string CodigoRegra { get; }
    public string MensagemDiagnostico { get; }
    public double? ValorObservado { get; }
    public double? ValorLimite { get; }
    public IReadOnlyDictionary<string, object>? Metadados { get; }
    public Guid? RegraQualidadeId { get; private init; }
    public string? NomeRegra { get; private init; }

    public ResultadoGateQualidade ComRegra(Guid regraQualidadeId, string nomeRegra) =>
        this with
        {
            RegraQualidadeId = regraQualidadeId,
            NomeRegra = nomeRegra
        };

    /// <summary>
    /// Cria um resultado normal, sem violação.
    /// </summary>
    public static ResultadoGateQualidade Normal(string tipoRegra)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoRegra);

        return new(
            false,
            NivelSeveridade.Normal,
            tipoRegra,
            "OK",
            "Operação dentro dos padrões normais.",
            null,
            null,
            null);
    }

    /// <summary>Indica que ainda faltam amostras, sem declarar uma violação térmica.</summary>
    public static ResultadoGateQualidade DadosInsuficientes(string tipoRegra, string mensagem) =>
        new(false, NivelSeveridade.Normal, tipoRegra, "DADOS_INSUFICIENTES", mensagem,
            null, null, new Dictionary<string, object> { ["avaliacaoConcluida"] = false });

    /// <summary>
    /// Cria um resultado que exige atenção, mas ainda não configura alerta preditivo.
    /// </summary>
    public static ResultadoGateQualidade Atencao(
        string tipoRegra,
        string codigoRegra,
        string mensagemDiagnostico,
        double? valorObservado = null,
        double? valorLimite = null,
        IReadOnlyDictionary<string, object>? metadados = null) =>
        CriarViolacao(
            NivelSeveridade.Atencao,
            tipoRegra,
            codigoRegra,
            mensagemDiagnostico,
            valorObservado,
            valorLimite,
            metadados);

    /// <summary>
    /// Cria um resultado de alerta preditivo.
    /// </summary>
    public static ResultadoGateQualidade AlertaPreditivo(
        string tipoRegra,
        string codigoRegra,
        string mensagemDiagnostico,
        double? valorObservado = null,
        double? valorLimite = null,
        IReadOnlyDictionary<string, object>? metadados = null) =>
        CriarViolacao(
            NivelSeveridade.AlertaPreditivo,
            tipoRegra,
            codigoRegra,
            mensagemDiagnostico,
            valorObservado,
            valorLimite,
            metadados);

    /// <summary>
    /// Cria um resultado crítico.
    /// </summary>
    public static ResultadoGateQualidade Critico(
        string tipoRegra,
        string codigoRegra,
        string mensagemDiagnostico,
        double? valorObservado = null,
        double? valorLimite = null,
        IReadOnlyDictionary<string, object>? metadados = null) =>
        CriarViolacao(
            NivelSeveridade.Critico,
            tipoRegra,
            codigoRegra,
            mensagemDiagnostico,
            valorObservado,
            valorLimite,
            metadados);

    private static ResultadoGateQualidade CriarViolacao(
        NivelSeveridade severidade,
        string tipoRegra,
        string codigoRegra,
        string mensagemDiagnostico,
        double? valorObservado,
        double? valorLimite,
        IReadOnlyDictionary<string, object>? metadados)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoRegra);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoRegra);
        ArgumentException.ThrowIfNullOrWhiteSpace(mensagemDiagnostico);

        return new(
            true,
            severidade,
            tipoRegra,
            codigoRegra,
            mensagemDiagnostico,
            valorObservado,
            valorLimite,
            metadados);
    }
}
