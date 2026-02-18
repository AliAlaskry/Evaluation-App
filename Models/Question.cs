public class Question
{
    public string Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public double Weight { get; set; } = 0;
    public bool TeamLeaderOnly { get; set; } = false;
    public bool Include { get; set; } = true;
    public double MinValue { get; set; } = 0;
    public double MaxValue { get; set; } = 100;
    public double DefaultValue { get; set; } = 0;
    public string ScoreFormula { get; set; }
    public string CombinedScoreFormula { get; set; }
    public string MinValueMeaning { get; set; } = string.Empty;
    public string MaxValueMeaning { get; set; } = string.Empty;

    public double Value { get; set; } = 0;
    public double Score { get; set; } = 0;

    public void CalculateScore()
    {
        var scoring = ConfigLoader.EmployeeEvaluationConfig.Scoring;
        string? formula = ScoreFormula ?? scoring.DefaultQuestionScoreFormula;
        double fallback = Value;

        Score = FormulaEngine.EvaluateToScalar(formula, DefaultVariables(this), fallback);
    }
    public static double CalculateCombinedScore(Question question, IReadOnlyList<double> scores)
    {
        var scoring = ConfigLoader.EmployeeEvaluationConfig.Scoring;
        string? formula = question.CombinedScoreFormula ?? scoring.DefaultCombinedQuestionScoreFormula;
        double fallback = question.Value;

        return FormulaEngine.EvaluateToScalar(formula, 
            new Dictionary<string, FormulaEngine.Value>
            {
                ["Scores"] = new FormulaEngine.Value(scores),
            }, fallback);
    }

    public static Dictionary<string, FormulaEngine.Value> DefaultVariables(Question question)
    {
        return new Dictionary<string, FormulaEngine.Value>
        {
            ["Value"] = new FormulaEngine.Value(question.Value),
            ["MinValue"] = new FormulaEngine.Value(question.MinValue),
            ["MaxValue"] = new FormulaEngine.Value(question.MaxValue),
            ["Default"] = new FormulaEngine.Value(question.DefaultValue),
            ["QuestionWeight"] = new FormulaEngine.Value(question.Weight),
        };
    }

    public Question Clone()
    {
        return new Question
        {
            Id = Id,
            Text = Text,
            Weight = Weight,
            TeamLeaderOnly = TeamLeaderOnly,
            Include = Include,
            MinValue = MinValue,
            MaxValue = MaxValue,
            DefaultValue = DefaultValue,
            ScoreFormula = ScoreFormula,
            CombinedScoreFormula = CombinedScoreFormula,
            MinValueMeaning = MinValueMeaning,
            MaxValueMeaning = MaxValueMeaning,
            Value = Value,
            Score = Score
        };
    }
}
