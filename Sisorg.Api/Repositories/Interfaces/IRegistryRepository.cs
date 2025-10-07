using Sisorg.Api.Domain.Entities;

namespace Sisorg.Api.Repositories;

public interface IRegistryRepository
{
    Task<Registry> AddAsync(Registry entity, CancellationToken ct);
    Task<Registry?> GetAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
