using Newtonsoft.Json;

public class EmployeeEvaluationResult
{
    [JsonConstructor]
    public EmployeeEvaluationResult(Employee emp, List<Section> sections)
    {
        Employee = emp;
        Sections = sections;
        Questions = sections.SelectMany(o => o.Questions).ToDictionary(o => o.Id, o => o);
        SetTotalScore();
    }

    public Employee Employee { get; set; }
    public List<Section> Sections { get; set; } = new();
    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; } = false;
    public double TotalScore { get; private set; }

    [JsonIgnore]
    public Dictionary<string, Question> Questions { get; private set; }

    public void SetTotalScore()
    {
        var scoring = ConfigLoader.LoadEmployeeOptions().Scoring;

        foreach (var section in Sections)
        {
            foreach (var question in section.Questions)
                question.Score = Question.ScoreQuestion(question);

            section.SetTotalScore(scoring);
        }

        var activeSections = Sections.Where(s => s.Include && !s.TeamLeaderOnly).ToList();
        var sectionScores = activeSections.Select(s => s.TotalScore).ToList();
        var sectionWeights = activeSections.Select(s => (double)s.Weight).ToList();

        double defaultScore = 0;
        double sumWeights = sectionWeights.Sum();
        if (sumWeights > 0)
            defaultScore = activeSections.Sum(s => s.TotalScore * s.Weight) / sumWeights;

        TotalScore = FormulaEngine.EvaluateToScalar(scoring.TotalFormula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["SectionScore"] = new FormulaEngine.Value(sectionScores),
                ["SectionWeight"] = new FormulaEngine.Value(sectionWeights),
                ["SectionCount"] = new FormulaEngine.Value(sectionScores.Count)
            },
            defaultScore);
    }

    public void Reset()
    {
        foreach (var question in Questions.Values)
            question.Score = question.Default;

        SetTotalScore();
    }
}
