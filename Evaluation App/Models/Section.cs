public class Section
{
    public string Name { get; set; } = string.Empty;
    public float Weight { get; set; } = 1.0f;
    public bool ManagerOnly { get; set; } = false;
    public bool Include { get; set; } = true;
    public List<Question> Questions { get; set; } = new List<Question>();
    public double TotalScore { get; private set; }

    public void SetTotalScore()
    {
        TotalScore = CalculateTotalScore();
    }

    private double CalculateTotalScore()
    {
        var activeQuestions = Questions.Where(q => q.Include).ToList();
        double sumWeights = activeQuestions.Sum(q => q.Weight);

        if (sumWeights <= 0)
            return 0;

        double weightedSum = activeQuestions.Sum(q => q.Score * q.Weight);
        return weightedSum / sumWeights;
    }
}
