using Newtonsoft.Json;

public abstract class EvaluationBase
{
    [JsonConstructor]
    public EvaluationBase(Employee evaluator, List<Section> sections)
    {
        Evaluator = evaluator;
        Sections = sections;
        Questions = Sections.SelectMany(o => o.Questions).ToDictionary(o => o.Id, o => o);
        CalculateScore();
    }

    public const string EvaluationSperator = "Evaulated";
    protected const string ExcelPostfix = ".xlsx";

    public readonly Employee Evaluator;
    public readonly List<Section> Sections;

    [JsonIgnore]
    public Dictionary<string, Question> Questions { get; private set; }

    public abstract double Score { get; set; }

    public string Filename => BuildFilename();

    protected abstract string BuildFilename();

    protected abstract ScoringOptions ScoringOptions { get; }
    public void CalculateScore()
    {
        foreach (var section in Sections)
        {
            foreach (var question in section.Questions)
                question.CalculateScore();
            section.CalculateScore(ScoringOptions);
        }

        var sectionScores = Sections.Select(s => s.Score).ToList();
        var sectionWeights = Sections.Select(s => (double)s.Weight).ToList();

        double defaultScore = 0;
        double sumWeights = sectionWeights.Sum();
        if (sumWeights > 0)
            defaultScore = Sections.Sum(s => s.Score * s.Weight) / sumWeights;

        Score = FormulaEngine.EvaluateToScalar(ScoringOptions.TotalFormula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["SectionScore"] = new FormulaEngine.Value(sectionScores),
                ["SectionWeight"] = new FormulaEngine.Value(sectionWeights),
                ["SectionCount"] = new FormulaEngine.Value(sectionScores.Count)
            },
            defaultScore);
    }
    public void Reset()
    {
        foreach (var question in Questions.Values)
            question.Value = question.DefaultValue;
    }

    public static string BuildFilename(Employee evalutor)
    {
        return $"{evalutor.Title} {EvaluationSperator} {Constants.SYSTEM_EVALUATION_CODE}.xlsx";
    }
    public static string BuildFilename(Employee evalutor, Employee evaluted)
    {
        return $"{evalutor.Title} {EvaluationSperator} {evaluted.Title}.xlsx";
    }
}
