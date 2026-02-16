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

    public void SetTotalScore(NormalizationOptions? normalization = null)
    {
        TotalScore = CalculateTotalScore(normalization ?? new NormalizationOptions());
    }

    private double CalculateTotalScore(NormalizationOptions normalization)
    {
        var activeQuestions = Questions.Where(q => q.Include && !q.TeamLeaderOnly).ToList();
        double sumWeights = activeQuestions.Sum(q => q.Weight);

        if (sumWeights <= 0)
            return 0;

        double weightedSum = activeQuestions.Sum(q => NormalizeQuestionScore(q, normalization) * q.Weight);
        return weightedSum / sumWeights;
    }

    private static double NormalizeQuestionScore(Question question, NormalizationOptions normalization)
    {
        double range = question.Max - question.Min;
        if (range <= 0)
            return 0;

        double normalized = (question.Score - question.Min) / range;
        if (!normalization.HigherIsBetter)
            normalized = 1.0 - normalized;

        double scaleMin = normalization.ScaleMin;
        double scaleMax = normalization.ScaleMax;
        if (scaleMax < scaleMin)
            (scaleMin, scaleMax) = (scaleMax, scaleMin);

        double scaled = scaleMin + (Math.Clamp(normalized, 0, 1) * (scaleMax - scaleMin));
        return Math.Clamp(scaled, scaleMin, scaleMax);
    }
}
