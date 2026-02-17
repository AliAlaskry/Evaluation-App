public class Question
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double Weight { get; set; } = 0;
    public bool ManagerOnly { get; set; } = false;
    public bool TeamLeaderOnly { get; set; } = false;
    public bool Include { get; set; } = true;
    public double Min { get; set; } = 0;
    public double Max { get; set; } = 100;
    public double Default { get; set; } = 0;
    public string Formula { get; set; } = "QuestionScore = Value";
    public string MinMeaning { get; set; } = string.Empty;
    public string MaxMeaning { get; set; } = string.Empty;
    public double Score { get; set; } = 0;

    public static double ScoreQuestion(Question question, IReadOnlyList<double>? scores = null)
    {
        var scoring = ConfigLoader.LoadEmployeeOptions().Scoring;
        string? formula = question.Formula ?? scoring.DefaultQuestionFormula;
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
}
