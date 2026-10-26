using Hackatohm.Application.Abstractions.Persistence;

namespace Hackatohm.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
