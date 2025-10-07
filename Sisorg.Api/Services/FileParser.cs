using System.Text.RegularExpressions;
using Sisorg.Api.ViewModels;

namespace Sisorg.Api.Services;

public class FileParser : IFileParser
{
    private static readonly Regex NameRx  = new(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ValueRx = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ColorRx = new(@"^[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public async Task<List<RowViewModel>> ParseAsync(Stream file, CancellationToken ct = default)
    {
        using var reader = new StreamReader(file);
        var rows = new List<RowViewModel>();
        string? line;
        int n = 0;

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

            rows.Add(new RowViewModel
            {
                Name = name,
                Value = decimal.Parse(valueStr),
                Color = color.ToUpperInvariant()
            });
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("File has no valid rows.");

        return rows;
    }
}
