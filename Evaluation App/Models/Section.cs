public class Section
{
    public string Name { get; set; } = string.Empty;
    public string NumberMeaning { get; set; } = string.Empty;
    public float Weight { get; set; } = 1.0f;
    public bool ManagerOnly { get; set; } = false;
    public bool TeamLeaderOnly { get; set; } = false;
    public bool Include { get; set; } = true;
    public string? Formula { get; set; }
    public string? CombinedFormula { get; set; }
    public List<Question> Questions { get; set; } = new List<Question>();
    public double TotalScore { get; private set; }

    public void SetTotalScore(ScoringFormulaContext context)
    {
        var activeQuestions = Questions.Where(q => q.Include && !q.TeamLeaderOnly).ToList();
        var questionScores = activeQuestions.Select(q => q.Score).ToList();
        var questionWeights = activeQuestions.Select(q => q.Weight).ToList();

        double defaultScore = 0;
        double sumWeights = questionWeights.Sum();
        if (sumWeights > 0)
            defaultScore = activeQuestions.Sum(q => q.Score * q.Weight) / sumWeights;

        string? formula = context.UseCombinedFormulas ? (CombinedFormula ?? context.Scoring.CombinedSectionFormula ?? Formula ?? context.Scoring.SectionFormula)
                                                     : (Formula ?? context.Scoring.SectionFormula);

        TotalScore = FormulaEngine.EvaluateToScalar(formula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["QuestionScore"] = new FormulaEngine.Value(questionScores),
                ["QuestionWeight"] = new FormulaEngine.Value(questionWeights),
                ["QuestionCount"] = new FormulaEngine.Value(questionScores.Count),
                ["SectionWeight"] = new FormulaEngine.Value(Weight)
            },
            defaultScore);
    }
}
