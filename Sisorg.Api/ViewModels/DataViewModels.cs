namespace Sisorg.Api.ViewModels;

public record RowViewModel
{
    public string Name { get; init; } = default!;
    public decimal Value { get; init; }
    public string Color { get; init; } = default!;
}

public record DataViewModel
{
    public int ID { get; init; }
    public int Count { get; init; }
    public DateTime Timestamp { get; init; }
    public List<RowViewModel> Rows { get; init; } = new();
}
