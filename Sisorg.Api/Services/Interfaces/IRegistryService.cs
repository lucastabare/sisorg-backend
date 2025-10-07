using Sisorg.Api.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Sisorg.Api.Services;

public interface IRegistryService
{
    Task<DataViewModel> UploadAsync(IFormFile file, CancellationToken ct);
    Task<DataViewModel?> GetAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
