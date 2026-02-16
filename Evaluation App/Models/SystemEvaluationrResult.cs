using Newtonsoft.Json;

public class SystemEvaluationrResult
{
    [JsonConstructor]
    public SystemEvaluationrResult(string code, List<Section> sections)
    {
        Code = code;
        Sections = sections;
        Questions = sections.SelectMany(o => o.Questions).ToDictionary(o => o.Id, o => o);
        SetTotalScore();
    }

    public string Code { get; set; }
    public List<Section> Sections { get; set; } = new();
    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; }
    public double TotalScore { get; private set; }

    [JsonIgnore]
    public Dictionary<string, Question> Questions { get; private set; }

    public void SetTotalScore()
    {
        TotalScore = CalculateTotalScore();
    }

    private double CalculateTotalScore()
    {
        var normalization = ConfigLoader.LoadSystemOptions().Normalization;

        foreach (var section in Sections)
            section.SetTotalScore(normalization);

        var activeSections = Sections.Where(s => s.Include && !s.TeamLeaderOnly).ToList();
        double sumWeights = activeSections.Sum(s => s.Weight);

        if (sumWeights <= 0)
            return 0;

        double weightedSum = activeSections.Sum(s => s.TotalScore * s.Weight);
        return weightedSum / sumWeights;
    }

    public void Reset()
    {
        foreach (var question in Questions.Values)
            question.Score = question.Default;

        SetTotalScore();
    }
}
