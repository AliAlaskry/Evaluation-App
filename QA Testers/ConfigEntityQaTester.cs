using System;
using System.Collections.Generic;

public static class ConfigEntityQaTester
{
    public static ComparisonResult Compare(ConfigEntity a, ConfigEntity b)
    {
        var diffs = new List<string>();
        CompareInto(a, b, "ConfigEntity", diffs);
        return new ComparisonResult(diffs.Count == 0, diffs);
    }

    public static void CompareInto(ConfigEntity a, ConfigEntity b, string path, List<string> diffs)
    {
        CompareEntityBaseConfig(a.BaseConfig, b.BaseConfig, $"{path}.BaseConfig", diffs);
        CompareRootEntityConfig(a.RootConfig, b.RootConfig, $"{path}.RootConfig", diffs);
        CompareValueEntityConfig(a.ValueConfig, b.ValueConfig, $"{path}.ValueConfig", diffs);
        CompareAddonsEntityConfig(a.AddonsConfig, b.AddonsConfig, $"{path}.AddonsConfig", diffs);
    }

    private static void CompareEntityBaseConfig(EntityBaseConfig a, EntityBaseConfig b, string path, List<string> diffs)
    {
        CompareValue(a.ID, b.ID, $"{path}.ID", diffs);
        CompareDouble(a.Weight, b.Weight, $"{path}.Weight", diffs);
    }

    private static void CompareRootEntityConfig(RootEntityConfig? a, RootEntityConfig? b, string path, List<string> diffs)
    {
        if (!a.HasValue && !b.HasValue)
            return;

        if (!a.HasValue || !b.HasValue)
        {
            diffs.Add($"{path}: one has value and the other does not.");
            return;
        }

        CompareValue(a.Value.Title, b.Value.Title, $"{path}.Title", diffs);
        CompareValue(a.Value.ScoreMeaning, b.Value.ScoreMeaning, $"{path}.ScoreMeaning", diffs);

        var aChildren = a.Value.Childs;
        var bChildren = b.Value.Childs;

        if (aChildren == null && bChildren == null)
            return;

        if (aChildren == null || bChildren == null)
        {
            diffs.Add($"{path}.Childs: one is null and the other is not.");
            return;
        }

        CompareValue(aChildren.Count, bChildren.Count, $"{path}.Childs.Count", diffs);

        int count = Math.Min(aChildren.Count, bChildren.Count);
        for (int i = 0; i < count; i++)
            CompareInto(aChildren[i], bChildren[i], $"{path}.Childs[{i}]", diffs);
    }

    private static void CompareValueEntityConfig(ValueEntityConfig? a, ValueEntityConfig? b, string path, List<string> diffs)
    {
        if (!a.HasValue && !b.HasValue)
            return;

        if (!a.HasValue || !b.HasValue)
        {
            diffs.Add($"{path}: one has value and the other does not.");
            return;
        }

        CompareValue(a.Value.Body, b.Value.Body, $"{path}.Body", diffs);
        CompareValue(a.Value.MinValueMeaning, b.Value.MinValueMeaning, $"{path}.MinValueMeaning", diffs);
        CompareValue(a.Value.MaxValueMeaning, b.Value.MaxValueMeaning, $"{path}.MaxValueMeaning", diffs);
        CompareDouble(a.Value.MinValue, b.Value.MinValue, $"{path}.MinValue", diffs);
        CompareDouble(a.Value.MaxValue, b.Value.MaxValue, $"{path}.MaxValue", diffs);
        CompareDouble(a.Value.DefaultValue, b.Value.DefaultValue, $"{path}.DefaultValue", diffs);
    }

    private static void CompareAddonsEntityConfig(AddonsEntityConfig? a, AddonsEntityConfig? b, string path, List<string> diffs)
    {
        if (!a.HasValue && !b.HasValue)
            return;

        if (!a.HasValue || !b.HasValue)
        {
            diffs.Add($"{path}: one has value and the other does not.");
            return;
        }

        CompareNullableBool(a.Value.Include, b.Value.Include, $"{path}.Include", diffs);
        CompareStringList(a.Value.WhoAsk, b.Value.WhoAsk, $"{path}.WhoAsk", diffs);
        CompareStringList(a.Value.WhoNotAsk, b.Value.WhoNotAsk, $"{path}.WhoNotAsk", diffs);
        CompareStringList(a.Value.WhoAnswer, b.Value.WhoAnswer, $"{path}.WhoAnswer", diffs);
        CompareStringList(a.Value.WhoNotAnswer, b.Value.WhoNotAnswer, $"{path}.WhoNotAnswer", diffs);
        CompareValue(a.Value.SingleScoreFormula, b.Value.SingleScoreFormula, $"{path}.SingleScoreFormula", diffs);
        CompareValue(a.Value.CombinedScoreFormula, b.Value.CombinedScoreFormula, $"{path}.CombinedScoreFormula", diffs);
        CompareNullableBool(a.Value.ByTeamLeaderOnly, b.Value.ByTeamLeaderOnly, $"{path}.ByTeamLeaderOnly", diffs);
        CompareNullableBool(a.Value.ForTeamLeaderOnly, b.Value.ForTeamLeaderOnly, $"{path}.ForTeamLeaderOnly", diffs);
    }

    private static void CompareStringList(List<string>? a, List<string>? b, string path, List<string> diffs)
    {
        if (a == null && b == null)
            return;

        if (a == null || b == null)
        {
            diffs.Add($"{path}: one is null and the other is not.");
            return;
        }

        CompareValue(a.Count, b.Count, $"{path}.Count", diffs);

        int count = Math.Min(a.Count, b.Count);
        for (int i = 0; i < count; i++)
            CompareValue(a[i], b[i], $"{path}[{i}]", diffs);
    }

    private static void CompareNullableBool(bool? a, bool? b, string path, List<string> diffs)
    {
        if (a != b)
            diffs.Add($"{path}: '{a}' != '{b}'");
    }

    private static void CompareValue<T>(T a, T b, string path, List<string> diffs)
    {
        if (!EqualityComparer<T>.Default.Equals(a, b))
            diffs.Add($"{path}: '{a}' != '{b}'");
    }

    private static void CompareDouble(double a, double b, string path, List<string> diffs, double tolerance = 0.000001)
    {
        if (Math.Abs(a - b) > tolerance)
            diffs.Add($"{path}: '{a}' != '{b}'");
    }
}