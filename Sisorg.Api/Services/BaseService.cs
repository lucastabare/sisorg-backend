namespace Sisorg.Api.Services;

public abstract class BaseService
{
    protected (bool ok, string? error) GuardNotNull(object? obj, string name)
        => obj is null ? (false, $"{name} is required.") : (true, null);
}
