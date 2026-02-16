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
    public List<Section> Sections { get; set; } = new List<Section>();
    public string FinalNote { get; set; }
    public bool RecommendAsTeamLead { get; set; } = false;
    public double TotalScore { get; private set; }

    [JsonIgnore]
    public Dictionary<string, Question> Questions { get; private set; }

    public void SetTotalScore()
    {
        TotalScore = CalculateTotalScore();
    }
    private double CalculateTotalScore()
    {
        foreach (var section in Sections)
            section.SetTotalScore();

        // مجموع أوزان الأسئلة
        double sumWeights = Sections.Sum(q => q.Weight);

        // مجموع Score × Weight
        double weightedSum = Sections.Sum(q => q.TotalScore * q.Weight);

        // إذا مجموع الأوزان أقل من 1، نعتبر الباقي مكتمل (100%)
        if (sumWeights < 1.0)
        {
            double remainingWeight = 1.0 - sumWeights;
            weightedSum += remainingWeight * 100.0; // الباقي مكتمل 100%
        }

        // إذا مجموع الأوزان أكبر من 1 => خطأ
        if (sumWeights > 1.0)
        {
            MessageBox.Show($"Something wrong with section {Code}: sum of weights > 1");
        }

        return weightedSum;
    }
    public void Reset()
    {
        foreach(var question in Questions.Values)
        {
            question.Score = 0;
            SetTotalScore();
        }
    }
}
