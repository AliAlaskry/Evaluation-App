using Newtonsoft.Json;

internal static class ConfigLoader
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private static readonly string EmployeeEvaluationPath = Path.Combine(BasePath, "employee_evaluation_config.json");
    private static readonly string SystemConfigPath = Path.Combine(BasePath, "system_evaluation_config.json");

    private static EvaluationConfig<SystemEvaluationOptions>? systemEvaluationConfig;
    private static EvaluationConfig<EmployeeEvaluationOptions>? employeeEvaluationConfig;

    public static List<EntityBase> SystemEvaluationEntites
    {
        get
        {
            systemEvaluationConfig ??= new();
            if (systemEvaluationConfig == null || systemEvaluationConfig.Entities.Count == 0)
                Initialize();

            return [.. systemEvaluationConfig.Entities.Select(e => e.Clone())];
        }
    }
    public static List<EntityBase> EmployeeEvaluationEntites
    {
        get
        {
            employeeEvaluationConfig ??= new();
            if (employeeEvaluationConfig == null || employeeEvaluationConfig.Entities.Count == 0)
                Initialize();

            return [.. employeeEvaluationConfig.Entities.Select(e => e.Clone())];
        }
    }

    public static SystemEvaluationOptions SystemEvaluationOptions
    {
        get
        {
            systemEvaluationConfig ??= new();
            if (systemEvaluationConfig == null || systemEvaluationConfig.Entities.Count == 0)
                Initialize();

            return systemEvaluationConfig.Options;
        }
    }
    public static EmployeeEvaluationOptions EmployeeEvaluationOptions
    {
        get
        {
            employeeEvaluationConfig ??= new();
            if (employeeEvaluationConfig == null || employeeEvaluationConfig.Entities.Count == 0)
                Initialize();

            return employeeEvaluationConfig.Options;
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

            foreach (var entity in config.Entities)
                entity.Normalize();

            return config;
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new();
        }
    }
}


internal class EvaluationConfig<T> where T : EvaluationOptionsBase
{
    public T Options { get; set; } = default;
    public List<EntityBase> Entities { get; set; } = new();
}

internal abstract class EvaluationOptionsBase { }
internal class SystemEvaluationOptions : EvaluationOptionsBase
{
    public List<string> IssuesToResolve { get; set; } = new();
    public ScoringOptions Scoring { get; set; } = new();
}
internal class EmployeeEvaluationOptions : EvaluationOptionsBase
{
    public bool AskPreferTeamLeaderAssistant { get; set; } = false;
    public ScoringOptions Scoring { get; set; } = new();
}

internal struct ScoringOptions
{
    public string DefaultQuestionScoreFormula { get; set; }
    public string DefaultCombinedQuestionScoreFormula { get; set; }
    public string SectionFormula { get; set; }
    public string TotalFormula { get; set; }
}