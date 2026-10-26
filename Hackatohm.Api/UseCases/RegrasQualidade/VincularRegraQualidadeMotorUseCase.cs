using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Entities;
using Hackatohm.Api.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Hackatohm.Api.UseCases.RegrasQualidade;

public enum StatusVinculoRegraQualidade
{
    Criado,
    MotorNaoEncontrado,
    RegraNaoEncontrada,
    JaVinculada
}

public sealed record VincularRegraQualidadeResultado(
    StatusVinculoRegraQualidade Status,
    VinculoRegraQualidadeResponseDto? Vinculo = null);

public sealed class VincularRegraQualidadeMotorUseCase(
    IMotorRepository motorRepository,
    IRegraQualidadeRepository regraRepository,
    IMotorRegraQualidadeRepository vinculoRepository,
    IUnitOfWork unitOfWork,
    ILogger<VincularRegraQualidadeMotorUseCase> logger)
{
    public async Task<VincularRegraQualidadeResultado> ExecutarAsync(
        int motorId,
        Guid regraId,
        CriarVinculoRegraQualidadeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!await motorRepository.ExisteAsync(motorId, cancellationToken))
            return new(StatusVinculoRegraQualidade.MotorNaoEncontrado);

        var regra = await regraRepository.ObterPorIdAsync(regraId, cancellationToken);
        if (regra is null)
            return new(StatusVinculoRegraQualidade.RegraNaoEncontrada);

        if (await vinculoRepository.ObterAsync(motorId, regraId, cancellationToken) is not null)
            return new(StatusVinculoRegraQualidade.JaVinculada);

        var vinculo = new MotorRegraQualidade
        {
            MotorId = motorId,
            RegraQualidadeId = regraId,
            Ativo = request.Ativo,
            VinculadoEm = DateTimeOffset.UtcNow,
            RegraQualidade = regra
        };

        await vinculoRepository.AdicionarAsync(vinculo, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateLink(exception))
        {
            logger.LogWarning(
                "Conflito ao associar regra {RegraId} ao motor {MotorId}; a associação já existe.",
                regraId,
                motorId);
            throw new ApiConflictException(
                ApiErrorCatalog.QualityRuleAlreadyLinked,
                exception);
        }

        logger.LogInformation(
            "Regra {RegraId} associada ao motor {MotorId}. Ativo: {Ativo}.",
            regraId,
            motorId,
            vinculo.Ativo);
        return new(
            StatusVinculoRegraQualidade.Criado,
            RegraQualidadeResponseMapper.Mapear(vinculo));
    }

    private static bool IsDuplicateLink(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException &&
        postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
        postgresException.ConstraintName?.Contains(
            "motores_regras_qualidade",
            StringComparison.OrdinalIgnoreCase) == true;
}
