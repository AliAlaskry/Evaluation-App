using Evaluation_App.Services;
using Newtonsoft.Json;

public static class ConfigLoader
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private static readonly string EmployeeEvaluationPath = Path.Combine(BasePath, "employee_evaluation_config.json");
    private static readonly string SystemConfigPath = Path.Combine(BasePath, "system_evaluation_config.json");

    public static List<Section> LoadEmployeeSections()
    {
        return LoadConfigFromFile(EmployeeEvaluationPath).Sections;
    }

    public static List<Section> LoadSystemSections()
    {
        return LoadConfigFromFile(SystemConfigPath).Sections;
    }

    public static EmployeeEvaluationOptions LoadEmployeeOptions()
    {
        return LoadConfigFromFile(SystemConfigPath).Options;
    }

    private static EvaluationConfig LoadConfigFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"ملف الإعدادات غير موجود:\n{path}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new EvaluationConfig();
            }

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<EvaluationConfig>(json) ?? new EvaluationConfig();
            config.Sections ??= new List<Section>();
            config.Options ??= new EmployeeEvaluationOptions();

            return FilterSections(config);
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new EvaluationConfig();
        }
    }

    private static EvaluationConfig FilterSections(EvaluationConfig config)
    {
        var currentUser = AuthService.CurrentUser;

        config.Sections = config.Sections
            .Where(section => section.Include && (!section.ManagerOnly || currentUser.IsTeamLead))
            .Select(section =>
            {
                section.Questions = (section.Questions ?? new List<Question>())
                    .Where(q => q.Include && (!q.ManagerOnly || currentUser.IsTeamLead))
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
    public List<Section> Sections { get; set; } = new();
    public EmployeeEvaluationOptions Options { get; set; } = new();
}

public class EmployeeEvaluationOptions
{
    public bool AskPreferTeamLeaderAssistant { get; set; } = false;
    public List<string> IssuesToResolve { get; set; } = new();
}
