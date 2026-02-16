public class Section
{
    public string Name { get; set; } = string.Empty;
    public string NumberMeaning { get; set; } = string.Empty;
    public float Weight { get; set; } = 1.0f;
    public bool ManagerOnly { get; set; } = false;
    public bool TeamLeaderOnly { get; set; } = false;
    public bool Include { get; set; } = true;
    public List<Question> Questions { get; set; } = new List<Question>();
    public double TotalScore { get; private set; }

    public void SetTotalScore()
    {
        TotalScore = CalculateTotalScore();
    }

    private double CalculateTotalScore()
    {
        var activeQuestions = Questions.Where(q => q.Include && !q.TeamLeaderOnly).ToList();
        double sumWeights = activeQuestions.Sum(q => q.Weight);

        if (sumWeights <= 0)
            return 0;

        double weightedSum = activeQuestions.Sum(q => NormalizeQuestionScore(q) * q.Weight);
        return weightedSum / sumWeights;
    }

    private static double NormalizeQuestionScore(Question question)
    {
        double range = question.Max - question.Min;
        if (range <= 0)
            return 0;

        double normalized = (question.Score - question.Min) / range;
        return Math.Clamp(normalized * 100.0, 0, 100);
    }
}
