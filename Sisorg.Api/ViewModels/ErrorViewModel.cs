namespace Sisorg.Api.ViewModels;

public record ErrorViewModel
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
}
