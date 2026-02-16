public class Section
{
    public string Name { get; set; }
    public float Weight { get; set; } = 1.0f;
    public bool ManagerOnly { get; set; } = false;
    public List<Question> Questions { get; set; } = new List<Question>();
    public double TotalScore { get; private set; }

    public void SetTotalScore()
    {
        TotalScore = CalculateTotalScore();
    }
    private double CalculateTotalScore()
    {
        // مجموع أوزان الأسئلة
        double sumWeights = Questions.Sum(q => q.Weight);

        // مجموع Score × Weight
        double weightedSum = Questions.Sum(q => q.Score * q.Weight);

        // إذا مجموع الأوزان أقل من 1، نعتبر الباقي مكتمل (100%)
        if (sumWeights < 1.0)
        {
            double remainingWeight = 1.0 - sumWeights;
            weightedSum += remainingWeight * 100.0; // الباقي مكتمل 100%
        }

        // إذا مجموع الأوزان أكبر من 1 => خطأ
        if (sumWeights > 1.0)
        {
            MessageBox.Show($"Something wrong with section {Name}: sum of weights > 1");
        }

        return weightedSum;
    }
}