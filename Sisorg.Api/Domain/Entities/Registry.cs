namespace Sisorg.Api.Domain.Entities;

public class Registry
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public ICollection<RegistryRow> Rows { get; set; } = new List<RegistryRow>();
}

public class RegistryRow
{
    public int Id { get; set; }
    public int RegistryId { get; set; }
    public Registry Registry { get; set; } = default!;

    public string Name { get; set; } = default!;
    public decimal Value { get; set; }
    public string Color { get; set; } = default!; 
}
