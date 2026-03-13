using System;
using System.Collections.Generic;
using System.Linq;

public static class EvaluationInstanceQaTester
{
    public static ComparisonResult Compare(EvaluationInstance? a, EvaluationInstance? b)
    {
        var diffs = new List<string>();
        CompareEvaluationInstance(a, b, "EvaluationInstance", diffs);
        return new ComparisonResult(diffs.Count == 0, diffs);
    }

    private static void CompareEvaluationInstance(
        EvaluationInstance? a,
        EvaluationInstance? b,
        string path,
        List<string> diffs)
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

        EmployeeQaTester.CompareEmployee(a.Evaluator, b.Evaluator, $"{path}.Evaluator", diffs);
        EmployeeQaTester.CompareEmployee(a.BeingEvaluated, b.BeingEvaluated, $"{path}.BeingEvaluated", diffs);

        CompareValue(a.IsSystemEvaluation, b.IsSystemEvaluation, $"{path}.IsSystemEvaluation", diffs);
        CompareValue(a.FinalNote, b.FinalNote, $"{path}.FinalNote", diffs);
        if(!a.IsSystemEvaluation)
            CompareValue(a.RecommendAsTeamLead, b.RecommendAsTeamLead, $"{path}.RecommendAsTeamLead", diffs);
        if(a.IsSystemEvaluation)
        CompareValue(a.ReadyToBeAssistantTeamLeader, b.ReadyToBeAssistantTeamLeader, $"{path}.ReadyToBeAssistantTeamLeader", diffs);
        CompareDouble(a.TotalScore, b.TotalScore, $"{path}.TotalScore", diffs);
        CompareValue(a.FileName, b.FileName, $"{path}.FileName", diffs);

        CompareEntities(a.Entities, b.Entities, $"{path}.Entities", diffs);

        CompareEntityDictionary(a.EntitiesDict, b.EntitiesDict, $"{path}.EntitiesDict", diffs);
    }

    private static void CompareEntities(
        IReadOnlyList<EntityNode>? a,
        IReadOnlyList<EntityNode>? b,
        string path,
        List<string> diffs)
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
        {
            var result = EntityQaTester.Compare(a[i], b[i]);
            foreach (var diff in result.Differences)
                diffs.Add($"{path}[{i}].{diff.Replace("Entity.", "")}");
        }
    }

    private static void CompareEntityDictionary(
        IReadOnlyDictionary<string, EntityNode>? a,
        IReadOnlyDictionary<string, EntityNode>? b,
        string path,
        List<string> diffs)
    {
        if (a == null && b == null)
            return;

        if (a == null || b == null)
        {
            diffs.Add($"{path}: one is null and the other is not.");
            return;
        }

        CompareValue(a.Count, b.Count, $"{path}.Count", diffs);

        var aKeys = a.Keys.OrderBy(k => k).ToList();
        var bKeys = b.Keys.OrderBy(k => k).ToList();

        CompareStringLists(aKeys, bKeys, $"{path}.Keys", diffs);

        foreach (var key in aKeys.Intersect(bKeys))
        {
            var result = EntityQaTester.Compare(a[key], b[key]);
            foreach (var diff in result.Differences)
                diffs.Add($"{path}[\"{key}\"].{diff.Replace("Entity.", "")}");
        }
    }

    private static void CompareStringLists(
        IReadOnlyList<string>? a,
        IReadOnlyList<string>? b,
        string path,
        List<string> diffs)
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