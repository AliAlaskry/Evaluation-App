using Newtonsoft.Json;

public class SystemEvaluationrResult
{
    [JsonConstructor]
    public SystemEvaluationrResult(string code, List<Section> sections)
    {
        Code = code;
        Sections = sections;
        Questions = sections.SelectMany(o => o.Questions).ToDictionary(o => o.Id, o => o);
        SetTotalScore();
    }

    public string Code { get; set; }
    public List<Section> Sections { get; set; } = new();
    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; }
    public double TotalScore { get; private set; }

    [JsonIgnore]
    public Dictionary<string, Question> Questions { get; private set; }

    public void SetTotalScore()
    {
        var options = ConfigLoader.LoadSystemOptions();
        var context = new ScoringFormulaContext(options.Scoring, useCombinedFormulas: false);

        foreach (var section in Sections)
        {
            foreach (var question in section.Questions)
                question.Score = EvaluationResult.ScoreQuestion(question, options.Scoring, false);

            section.SetTotalScore(context);
        }

        var activeSections = Sections.Where(s => s.Include && !s.TeamLeaderOnly).ToList();
        var sectionScores = activeSections.Select(s => s.TotalScore).ToList();
        var sectionWeights = activeSections.Select(s => (double)s.Weight).ToList();

        double defaultScore = 0;
        double sumWeights = sectionWeights.Sum();
        if (sumWeights > 0)
            defaultScore = activeSections.Sum(s => s.TotalScore * s.Weight) / sumWeights;

        TotalScore = FormulaEngine.EvaluateToScalar(options.Scoring.TotalFormula,
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
            question.Score = question.Default;

        SetTotalScore();
    }
}
