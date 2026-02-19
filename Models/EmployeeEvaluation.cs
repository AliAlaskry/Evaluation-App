using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

public class EmployeeEvaluation : EvaluationBase, IEqualityComparer<EmployeeEvaluation>
{
    [JsonConstructor]
    public EmployeeEvaluation(Employee evaluator, List<Section> sections, Employee evaluated) 
        : base(evaluator, sections)
    {
        Evaluated = evaluated;
    }

    public readonly Employee Evaluated;

    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; } = false;
    public override double Score { get; set; } = 0;

    protected override ScoringOptions ScoringOptions => ConfigLoader.EmployeeEvaluationConfig.Scoring;

    protected override string BuildFilename()
    {
        return $"{Evaluator.Title} {EvaluationSperator} {Evaluated.Title}" + ExcelPostfix;
    }

    public EmployeeEvaluation Clone()
    {
        return new EmployeeEvaluation(Evaluator, Sections.Select(o => o.Clone()).ToList(), Evaluated)
        {
            FinalNote = FinalNote,
            RecommendAsTeamLead = RecommendAsTeamLead,
            Score = Score
        };
    }

    public bool Equals(EmployeeEvaluation? x, EmployeeEvaluation? y)
    {
        if(x == null || y == null) return false;

        return x.Evaluator.Code.Equals(y.Evaluator.Code) && x.Evaluated.Code.Equals(y.Evaluated.Code);
    }
    public int GetHashCode([DisallowNull] EmployeeEvaluation obj)
    {
        return base.GetHashCode();
    }
}
