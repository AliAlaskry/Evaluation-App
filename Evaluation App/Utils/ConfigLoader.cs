using Evaluation_App.Services;
using Newtonsoft.Json;

public static class ConfigLoader
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private static readonly string EmployeeEvaluationPath = Path.Combine(BasePath, "employee_evaluation_config.json");
    private static readonly string SystemConfigPath = Path.Combine(BasePath, "system_evaluation_config.json");

    public static List<Section> LoadEmployeeSections(Employee? targetEmployee = null)
    {
        return LoadConfigFromFile(EmployeeEvaluationPath, true, targetEmployee).Sections;
    }

    public static List<Section> LoadSystemSections()
    {
        return LoadConfigFromFile(SystemConfigPath, false, null).Sections;
    }

    public static EmployeeEvaluationOptions LoadEmployeeOptions()
    {
        return LoadConfigFromFile(EmployeeEvaluationPath, true, null).Options;
    }

    private static EvaluationConfig LoadConfigFromFile(string path, bool isEmployeeEvaluation, Employee? targetEmployee)
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

            return FilterSections(config, isEmployeeEvaluation, targetEmployee);
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new EvaluationConfig();
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
    public List<Section> Sections { get; set; } = new();
    public EmployeeEvaluationOptions Options { get; set; } = new();
}

public class EmployeeEvaluationOptions
{
    public bool AskPreferTeamLeaderAssistant { get; set; } = false;
    public List<string> IssuesToResolve { get; set; } = new();
}
