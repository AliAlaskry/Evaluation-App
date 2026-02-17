using Newtonsoft.Json;

public class EvaluationResult
{
    [JsonConstructor]
    public EvaluationResult(string code, bool isEmp, List<Section> sections)
    {
        Code = code;
        IsEmployeeEvaluation = isEmp;
        Sections = sections;
        Questions = sections.SelectMany(o => o.Questions).ToDictionary(o => o.Id, o => o);
        SetTotalScore();
    }

    public bool IsEmployeeEvaluation { get; set; }
    public string Code { get; set; }
    public List<Section> Sections { get; set; } = new();
    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; } = false;
    public double TotalScore { get; private set; }

    [JsonIgnore]
    public Dictionary<string, Question> Questions { get; private set; }

    public void SetTotalScore()
    {
        var options = ConfigLoader.LoadEmployeeOptions();
        var context = new ScoringFormulaContext(options.Scoring, useCombinedFormulas: false);

        foreach (var section in Sections)
        {
            foreach (var question in section.Questions)
                question.Score = ScoreQuestion(question, options.Scoring, false);

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

    public static double ScoreQuestion(Question question, ScoringOptions scoring, bool useCombinedFormula, IReadOnlyList<double>? scores = null)
    {
        string? formula = useCombinedFormula
            ? (question.CombinedFormula ?? scoring.DefaultCombinedQuestionFormula ?? question.Formula)
            : (question.Formula ?? scoring.DefaultQuestionFormula);

        var scoreList = (scores ?? new[] { question.Score }).ToList();
        double fallback = question.Score;

        return Math.Clamp(FormulaEngine.EvaluateToScalar(formula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["Scores"] = new FormulaEngine.Value(scoreList),
                ["Value"] = new FormulaEngine.Value(question.Score),
                ["Min"] = new FormulaEngine.Value(question.Min),
                ["Max"] = new FormulaEngine.Value(question.Max),
                ["Default"] = new FormulaEngine.Value(question.Default),
                ["QuestionWeight"] = new FormulaEngine.Value(question.Weight)
            },
            fallback), question.Min, question.Max);
    }

    public void Reset()
    {
        foreach (var question in Questions.Values)
            question.Score = question.Default;

        SetTotalScore();
    }
}
