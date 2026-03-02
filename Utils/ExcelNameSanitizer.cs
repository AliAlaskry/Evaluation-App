using ClosedXML.Excel;

public sealed class SheetIndex
{
    public Dictionary<int, Dictionary<string, int>> Columns { get; } = new()
    {
        {1, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) },
        {2, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) },
        {3, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) },
    };
}

public static class SheetIndexBuilder
{
    public static SheetIndex Build(IXLWorksheet ws)
    {
        var idx = new SheetIndex();

        foreach (var row in ws.RowsUsed())
        {
            for (int i = 1; i <= 3; i++)
            {
                var temp = row.Cell(i).GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(temp) && !idx.Columns[i].ContainsKey(temp))
                    idx.Columns[i][temp] = row.RowNumber();
            }
        }

        return idx;
    }
}