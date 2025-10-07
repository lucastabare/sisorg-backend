using System.Text.RegularExpressions;
using Sisorg.Api.Domain.Entities;
using Sisorg.Api.Repositories;
using Sisorg.Api.ViewModels;

namespace Sisorg.Api.Services;

public class RegistryService : BaseService, IRegistryService
{
    private static readonly Regex NameRx  = new(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ValueRx = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ColorRx = new(@"^[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    private readonly IRegistryRepository _repo;
    public RegistryService(IRegistryRepository repo) => _repo = repo;

    public async Task<DataViewModel> UploadAsync(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("No file was uploaded.");

        using var reader = new StreamReader(file.OpenReadStream());
        var rowsVm = new List<RowViewModel>();
        string? line; int n = 0;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            ct.ThrowIfCancellationRequested();
            n++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('#');
            if (parts.Length != 3)
                throw new InvalidOperationException($"Line {n}: invalid format. Expected 'Name#Value#Color'.");

            var name = parts[0].Trim();
            var valueStr = parts[1].Trim();
            var color = parts[2].Trim();

            if (!NameRx.IsMatch(name))
                throw new InvalidOperationException($"Line {n}: invalid name. Only letters and numbers are allowed.");
            if (!ValueRx.IsMatch(valueStr))
                throw new InvalidOperationException($"Line {n}: invalid value. Only digits are allowed.");
            if (!ColorRx.IsMatch(color))
                throw new InvalidOperationException($"Line {n}: invalid color hex. Expect 6 hex digits without '#'.");

            rowsVm.Add(new RowViewModel { Name = name, Value = decimal.Parse(valueStr), Color = color.ToUpperInvariant() });
        }

        if (rowsVm.Count == 0)
            throw new InvalidOperationException("File has no valid rows.");

        var entity = new Registry
        {
            TimestampUtc = DateTime.UtcNow,
            Rows = rowsVm.Select(r => new RegistryRow { Name = r.Name, Value = r.Value, Color = r.Color }).ToList()
        };

        var saved = await _repo.AddAsync(entity, ct);

        return new DataViewModel
        {
            ID = saved.Id,
            Count = saved.Rows.Count,
            Timestamp = saved.TimestampUtc,
            Rows = rowsVm
        };
    }

    public async Task<DataViewModel?> GetAsync(int id, CancellationToken ct)
    {
        var e = await _repo.GetAsync(id, ct);
        return e is null ? null : new DataViewModel
        {
            ID = e.Id,
            Count = e.Rows.Count,
            Timestamp = e.TimestampUtc,
            Rows = e.Rows.Select(x => new RowViewModel { Name = x.Name, Value = x.Value, Color = x.Color }).ToList()
        };
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct) => _repo.DeleteAsync(id, ct);
}
