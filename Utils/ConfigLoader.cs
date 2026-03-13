using Newtonsoft.Json;
using System.Windows.Forms.VisualStyles;

internal static class ConfigLoader
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private static readonly string EmployeeEvaluationPath = Path.Combine(BasePath, "employee_evaluation_config.json");
    private static readonly string SystemConfigPath = Path.Combine(BasePath, "system_evaluation_config.json");

    private static EvaluationConfig<SystemEvaluationOptions> systemEvaluationConfig;
    private static EvaluationConfig<EmployeeEvaluationOptions> employeeEvaluationConfig;

    public static List<ConfigEntity> SystemEvaluationEntites=> 
        [.. systemEvaluationConfig.Entities.Select(e => e.Clone())];
    public static List<ConfigEntity> EmployeeEvaluationEntites =>
        [.. employeeEvaluationConfig.Entities.Select(e => e.Clone())];

    public static SystemEvaluationOptions SystemEvaluationOptions => systemEvaluationConfig.Options;
    public static EmployeeEvaluationOptions EmployeeEvaluationOptions => employeeEvaluationConfig.Options;

    public static void Initialize()
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
                entity.NormalizeAll();

            return new EvaluationConfig<T>(config.Options, config.Entities);
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
    public EvaluationConfig() { }
    public EvaluationConfig(T options, List<ConfigEntity> entities)
    {
        this.Options = options;
        this.Entities = entities;
        this.EntitiesDict = FlattenAll(entities).ToDictionary(o => o.BaseConfig.ID, o => o);
    }

    [JsonProperty("Options")]
    public T Options { get; private set; }

    [JsonProperty("Entities")]
    public List<ConfigEntity> Entities { get; private set; }

    public Dictionary<string, ConfigEntity> EntitiesDict { get; private set; }

    private static List<ConfigEntity> FlattenAll(List<ConfigEntity>? roots)
    {
        var result = new List<ConfigEntity>();

        if (roots == null)
            return result;

        foreach (var root in roots)
            AddRecursive(root, result);

        return result;
    }
    private static void AddRecursive(ConfigEntity entity, List<ConfigEntity> list)
    {
        list.Add(entity);

        if (entity.RootConfig.HasValue && entity.RootConfig.Value.HasChilds)
        {
            foreach (var child in entity.RootConfig.Value.Childs)
                AddRecursive(child, list);
        }
    }
}

public abstract class EvaluationOptionsBase { }
public class SystemEvaluationOptions : EvaluationOptionsBase
{
    [JsonProperty("IssuesToResolve")]
    public List<string> IssuesToResolve { get; private set; } = new();

    [JsonProperty("Scoring")]
    public ScoringOptions Scoring { get; private set; } = new();
}
public class EmployeeEvaluationOptions : EvaluationOptionsBase
{
    [JsonProperty("AskPreferTeamLeaderAssistant")]
    public bool AskPreferTeamLeaderAssistant { get; private set; } = false;

    [JsonProperty("Scoring")]
    public ScoringOptions Scoring { get; private set; } = new();
}

public struct ScoringOptions
{
    [JsonProperty("DefaultQuestionScoreFormula")]
    public string DefaultQuestionScoreFormula { get; private set; }

    [JsonProperty("DefaultCombinedQuestionScoreFormula")]
    public string DefaultCombinedQuestionScoreFormula { get; private set; }

    [JsonProperty("SectionFormula")]
    public string SectionFormula { get; private set; }

    [JsonProperty("TotalFormula")]
    public string TotalFormula { get; private set; }
}