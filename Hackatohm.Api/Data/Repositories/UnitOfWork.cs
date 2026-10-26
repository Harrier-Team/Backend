using Hackatohm.Api.Data.Interfaces;

namespace Hackatohm.Api.Data.Repositories;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
