using System;
using System.Collections.Generic;

public static class EmployeeQaTester
{
    public static ComparisonResult Compare(Employee? a, Employee? b)
    {
        var diffs = new List<string>();
        CompareEmployee(a, b, "Employee", diffs);
        return new ComparisonResult(diffs.Count == 0, diffs);
    }

    public static void CompareEmployee(Employee? a, Employee? b, string path, List<string> diffs)
    {
        if (ReferenceEquals(a, b))
            return;

        if (a is null && b is null)
            return;

        if (a is null || b is null)
        {
            diffs.Add($"{path}: one is null and the other is not.");
            return;
        }

        CompareValue(a.Code, b.Code, $"{path}.Code", diffs);
        CompareValue(a.Name, b.Name, $"{path}.Name", diffs);
        CompareValue(a.IsTeamLead, b.IsTeamLead, $"{path}.IsTeamLead", diffs);
        CompareValue(a.Include, b.Include, $"{path}.Include", diffs);
        CompareValue(a.Title, b.Title, $"{path}.Title", diffs);
    }

    private static void CompareValue<T>(T a, T b, string path, List<string> diffs)
    {
        if (!EqualityComparer<T>.Default.Equals(a, b))
            diffs.Add($"{path}: '{a}' != '{b}'");
    }
}