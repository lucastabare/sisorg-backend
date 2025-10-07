using Microsoft.EntityFrameworkCore;
using Sisorg.Api.Domain.Entities;
using Sisorg.Api.Infrastructure;

namespace Sisorg.Api.Repositories;

public class RegistryRepository : IRegistryRepository
{
    private readonly AppDbContext _db;
    public RegistryRepository(AppDbContext db) => _db = db;

    public async Task<Registry> AddAsync(Registry entity, CancellationToken ct)
    {
        _db.Registries.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public Task<Registry?> GetAsync(int id, CancellationToken ct) =>
        _db.Registries.Include(r => r.Rows).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Registries.Include(r => r.Rows).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return false;
        _db.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
