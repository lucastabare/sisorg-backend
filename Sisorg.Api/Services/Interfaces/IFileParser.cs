using Sisorg.Api.ViewModels;

namespace Sisorg.Api.Services;

public interface IFileParser
{
    Task<List<RowViewModel>> ParseAsync(Stream file, CancellationToken ct = default);
}
