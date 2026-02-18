using Evaluation_App.Services;
using Newtonsoft.Json;

public static class ConfigLoader
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private static readonly string EmployeeEvaluationPath = Path.Combine(BasePath, "employee_evaluation_config.json");
    private static readonly string SystemConfigPath = Path.Combine(BasePath, "system_evaluation_config.json");

    private static EvaluationConfig<SystemEvaluationOptions> systemEvaluationConfig;
    private static EvaluationConfig<EmployeeEvaluationOptions> employeeEvaluationConfig;

    public static EvaluationConfig<SystemEvaluationOptions> SystemEvaluationConfig
    {
        get
        {
            systemEvaluationConfig ??= new();
            if (!systemEvaluationConfig.Sections.Any())
                Initialize();

            return systemEvaluationConfig.Clone();
        }
    }
    public static EvaluationConfig<EmployeeEvaluationOptions> EmployeeEvaluationConfig
    {
        get
        {
            employeeEvaluationConfig ??= new();
            if (!employeeEvaluationConfig.Sections.Any())
                Initialize();

            return employeeEvaluationConfig.Clone();
        }
    }

    private static void Initialize()
    {
        systemEvaluationConfig = LoadEvaluationConfig<SystemEvaluationOptions>(SystemConfigPath);
        employeeEvaluationConfig = LoadEvaluationConfig<EmployeeEvaluationOptions>(EmployeeEvaluationPath);
    }

    private static EvaluationConfig<T> LoadEvaluationConfig<T>(string path) where T : EvaluationOptionsBase
    {
        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"ملف الإعدادات غير موجود:\n{path}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new();
            }

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<EvaluationConfig<T>>(json) ?? new();

            config.Sections = config.Sections
                .Where(s => s.Include)
                .Select(section =>
                 {
                     section.Questions = section.Questions.Where(q => q.Include).ToList();
                     return section;
                 })
                .Where(section => section.Questions.Any())
                .ToList();

            return config;
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new();
        }
    }
}


public class EvaluationConfig<T> where T : EvaluationOptionsBase
{
    public T Options { get; set; } = default;
    public List<Section> Sections { get; set; } = new();
    public List<Section> FilteredSectionsForCurrentUser { get; set; } = new();
    public ScoringOptions Scoring { get; set; } = new();

    public List<Section> FilterSectionsForEmployee(Employee employee)
    {
        var sections = Sections
            .Where(section => !section.TeamLeaderOnly || employee.IsTeamLead)
            .Select(section =>
            {
                section.Questions = section.Questions
                    .Where(q => !q.TeamLeaderOnly || employee.IsTeamLead)
                    .Select(NormalizeQuestion)
                    .ToList();
                return section;
            })
            .Where(section => section.Questions.Any())
            .ToList();

        return sections;
    }

    private Question NormalizeQuestion(Question question)
    {
        if (question.MaxValue < question.MinValue)
            (question.MinValue, question.MaxValue) = (question.MaxValue, question.MinValue);

        question.DefaultValue = Math.Clamp(question.DefaultValue, question.MinValue, question.MaxValue);
        question.Value = Math.Clamp(question.Value, question.MinValue, question.MaxValue);
        return question;
    }

    public EvaluationConfig<T> Clone()
    {
        return new EvaluationConfig<T>
        {
            Options = this.Options,
            Sections = this.Sections,
            FilteredSectionsForCurrentUser = FilterSectionsForEmployee(AuthService.CurrentUser),
            Scoring = this.Scoring
        };
    }
}

public abstract class EvaluationOptionsBase { }
public class SystemEvaluationOptions : EvaluationOptionsBase
{
    public List<string> IssuesToResolve { get; set; } = new();
    public ScoringOptions Scoring { get; set; } = new();
}
public class EmployeeEvaluationOptions : EvaluationOptionsBase
{
    public bool AskPreferTeamLeaderAssistant { get; set; } = false;
    public ScoringOptions Scoring { get; set; } = new();
}

public class ScoringOptions
{
    public string DefaultQuestionScoreFormula { get; set; }
    public string DefaultCombinedQuestionScoreFormula { get; set; }
    public string SectionFormula { get; set; }
    public string TotalFormula { get; set; }
}