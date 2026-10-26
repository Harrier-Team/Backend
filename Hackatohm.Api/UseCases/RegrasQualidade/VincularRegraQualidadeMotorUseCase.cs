using Hackatohm.Api.Data.Interfaces;
using Hackatohm.Api.DTOs;
using Hackatohm.Api.Entities;

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
    IUnitOfWork unitOfWork)
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
            VinculadoEm = DateTime.UtcNow,
            RegraQualidade = regra
        };

        await vinculoRepository.AdicionarAsync(vinculo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(
            StatusVinculoRegraQualidade.Criado,
            RegraQualidadeResponseMapper.Mapear(vinculo));
    }
}
