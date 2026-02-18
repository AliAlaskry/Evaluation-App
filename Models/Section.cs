public class Section : IEqualityComparer<Section>
{
    public string Name { get; set; } = string.Empty;
    public float Weight { get; set; } = 1.0f;
    public bool Include { get; set; } = true;
    public bool TeamLeaderOnly { get; set; } = false;
    public double MinValue { get; set; } = 0;
    public double MaxValue { get; set; } = 100;
    public string ScoreFormula { get; set; }
    public string ScoreMeaning { get; set; } = string.Empty;

    public List<Question> Questions { get; set; } = new List<Question>();
    public double Score { get; set; } = 0;

    public void CalculateScore(ScoringOptions scoring)
    {
        var questionScores = Questions.Select(q => q.Value).ToList();
        var questionWeights = Questions.Select(q => q.Weight).ToList();

        double defaultScore = 0;
        double sumWeights = questionWeights.Sum();
        if (sumWeights > 0)
            defaultScore = Questions.Sum(q => q.Value * q.Weight) / sumWeights;

        string? formula = ScoreFormula ?? scoring.SectionFormula;

        Score = FormulaEngine.EvaluateToScalar(formula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["QuestionScore"] = new FormulaEngine.Value(questionScores),
                ["QuestionWeight"] = new FormulaEngine.Value(questionWeights),
                ["QuestionCount"] = new FormulaEngine.Value(questionScores.Count),
                ["SectionWeight"] = new FormulaEngine.Value(Weight)
            },
            defaultScore);
    }

    public Section Clone()
    {
        return new Section
        {
            Name = Name,
            Weight = Weight,
            Include = Include,
            TeamLeaderOnly = TeamLeaderOnly,
            MinValue = MinValue,
            MaxValue = MaxValue,
            ScoreFormula = ScoreFormula,
            ScoreMeaning = ScoreMeaning,
            Questions = CloneQuestions(),
            Score = Score,
        };
    }
    public List<Question> CloneQuestions()
    {
        List<Question> list = new();
        foreach (Question question in Questions)
            list.Add(question.Clone());
        return list;
    }

    public bool Equals(Section? x, Section? y)
    {
        if(x == null || y == null) return false;

        return x.Name.Equals(y.Name);
    }

    public int GetHashCode(Section obj)
    {
        return base.GetHashCode();
    }
}
