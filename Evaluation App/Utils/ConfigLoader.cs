using Evaluation_App.Services;
using Newtonsoft.Json;

public static class ConfigLoader
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private static readonly string EmployeeEvaluationPath = Path.Combine(BasePath, "employee_evaluation_config.json");
    private static readonly string SystemConfigPath = Path.Combine(BasePath, "system_evaluation_config.json");

    public static List<Section> LoadEmployeeSections(Employee? targetEmployee = null)
    {
        return LoadSectionsFromFile(EmployeeEvaluationPath, true, targetEmployee).Sections;
    }

    public static List<Section> LoadSystemSections()
    {
        return LoadSectionsFromFile(SystemConfigPath, false, null).Sections;
    }

    public static SystemOptions LoadSystemOptions()
    {
        var options = LoadOptionsFromFile(SystemConfigPath).Options;
        return new SystemOptions
        {
            IssuesToResolve = options.IssuesToResolve,
            Normalization = options.Normalization,
            Scoring = options.Scoring
        };
    }

    public static EmployeeOptions LoadEmployeeOptions()
    {
        var options = LoadOptionsFromFile(EmployeeEvaluationPath).Options;
        return new EmployeeOptions
        {
            AskPreferTeamLeaderAssistant = options.AskPreferTeamLeaderAssistant,
            Normalization = options.Normalization,
            IssuesToResolve = options.IssuesToResolve,
            Scoring = options.Scoring
        };
    }

    private static EvaluationConfig LoadSectionsFromFile(string path, bool isEmployeeEvaluation, Employee? targetEmployee)
    {
        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"ملف الإعدادات غير موجود:\n{path}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new();
            }

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<EvaluationConfig>(json) ?? new();

            return FilterSections(config, isEmployeeEvaluation, targetEmployee);
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new();
        }
    }

    private static EvaluationConfig LoadOptionsFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"ملف الإعدادات غير موجود:\n{path}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new();
            }

            var json = File.ReadAllText(path);
            var options = JsonConvert.DeserializeObject<EvaluationConfig>(json) ?? new();

            return options;
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new();
        }
    }

    private static EvaluationConfig FilterSections(EvaluationConfig config, bool isEmployeeEvaluation, Employee? targetEmployee)
    {
        var currentUser = AuthService.CurrentUser;
        bool canSeeTeamLeaderOnly = isEmployeeEvaluation &&
                                     currentUser.IsTeamLead &&
                                     targetEmployee != null &&
                                     !string.Equals(currentUser.Code, targetEmployee.Code, StringComparison.OrdinalIgnoreCase);

        config.Sections = config.Sections
            .Where(section => section.Include && (!section.ManagerOnly || currentUser.IsTeamLead) && (!section.TeamLeaderOnly || canSeeTeamLeaderOnly))
            .Select(section =>
            {
                section.Questions = (section.Questions ?? new List<Question>())
                    .Where(q => q.Include && (!q.ManagerOnly || currentUser.IsTeamLead) && (!q.TeamLeaderOnly || canSeeTeamLeaderOnly))
                    .Select(NormalizeQuestion)
                    .ToList();
                return section;
            })
            .Where(section => section.Questions.Any())
            .ToList();

        return config;
    }

    private static Question NormalizeQuestion(Question question)
    {
        if (question.Max < question.Min)
            (question.Min, question.Max) = (question.Max, question.Min);

        question.Default = Math.Clamp(question.Default, question.Min, question.Max);
        question.Score = Math.Clamp(question.Score == 0 ? question.Default : question.Score, question.Min, question.Max);
        return question;
    }
}

public class EvaluationConfig
{
    public EvaluationOptions Options { get; set; } = new();
    public List<Section> Sections { get; set; } = new();
}

public class EvaluationOptions
{
    public List<string> IssuesToResolve { get; set; } = new();
    public bool AskPreferTeamLeaderAssistant { get; set; } = false;
    public NormalizationOptions Normalization { get; set; } = new();
    public ScoringOptions Scoring { get; set; } = new();
}

public class SystemOptions
{
    public List<string> IssuesToResolve { get; set; } = new();
    public NormalizationOptions Normalization { get; set; } = new();
    public ScoringOptions Scoring { get; set; } = new();
}

public class EmployeeOptions : SystemOptions
{
    public bool AskPreferTeamLeaderAssistant { get; set; } = false;
}

public class ScoringOptions
{
    public string DefaultQuestionFormula { get; set; } = "QuestionScore = Value";
    public string DefaultCombinedQuestionFormula { get; set; } = "QuestionScore = median(Scores)";
    public string SectionFormula { get; set; } = "SectionScore = sum(QuestionScore * QuestionWeight) / sum(QuestionWeight)";
    public string CombinedSectionFormula { get; set; } = "SectionScore = sum(QuestionScore * QuestionWeight) / sum(QuestionWeight)";
    public string TotalFormula { get; set; } = "TotalScore = sum(SectionScore * SectionWeight) / sum(SectionWeight)";
    public string CombinedTotalFormula { get; set; } = "TotalScore = sum(SectionScore * SectionWeight) / sum(SectionWeight)";
}

public readonly record struct ScoringFormulaContext(ScoringOptions scoring, bool useCombinedFormulas);

public class NormalizationOptions
{
    public double ScaleMin { get; set; } = 0;
    public double ScaleMax { get; set; } = 100;
    public bool HigherIsBetter { get; set; } = true;
}
