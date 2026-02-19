using System.Diagnostics.CodeAnalysis;

public class SystemEvaluation : EvaluationBase, IEqualityComparer<SystemEvaluation>
{
    public SystemEvaluation(Employee evalutor, List<Section> sections) 
        : base(evalutor, sections) { }

    public string FinalNote { get; set; } = string.Empty;
    public bool ReadyToBeAssistantTeamLeader { get; set; }
    public override double Score { get; set; } = 0;

    protected override ScoringOptions ScoringOptions => ConfigLoader.SystemEvaluationConfig.Scoring;

    protected override string BuildFilename()
    {
        return $"{Evaluator.Title} {EvaluationSperator} {Constants.SYSTEM_EVALUATION_CODE}" + ExcelPostfix;
    }

    public SystemEvaluation Clone()
    {
        return new SystemEvaluation(Evaluator, Sections.Select(o => o.Clone()).ToList())
        {
            Score = Score,
            FinalNote = FinalNote,
            ReadyToBeAssistantTeamLeader = ReadyToBeAssistantTeamLeader,
        };
    }

    public bool Equals(SystemEvaluation? x, SystemEvaluation? y)
    {
        if (x == null || y == null) return false;

        return x.Evaluator.Code.Equals(y.Evaluator.Code);
    }

    public int GetHashCode([DisallowNull] SystemEvaluation obj)
    {
        return base.GetHashCode();
    }
}
