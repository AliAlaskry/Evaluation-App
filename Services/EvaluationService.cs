using Newtonsoft.Json;
using static Constants;

public static class EvaluationService
{
    public static void Save(EmployeeEvaluationResult eval)
    {
        SaveEvaluation(eval.Employee.Code, eval);
    }

    public static void Save(SystemEvaluationrResult eval)
    {
        SaveEvaluation(SYSTEM_EVALUATION_CODE, eval);
    }

    private static void SaveEvaluation(string code, object evaluation)
    {
        if (!Directory.Exists(EvaluationPath))
            Directory.CreateDirectory(EvaluationPath);

        string file = Path.Combine(EvaluationPath, $"{code}.json");
        File.WriteAllText(file, JsonConvert.SerializeObject(evaluation, Formatting.Indented));
    }

    public static EmployeeEvaluationResult LoadEvaluation(string employeeCode)
    {
        string file = Path.Combine(EvaluationPath, $"{employeeCode}.json");
        if (!File.Exists(file)) return null;

        return JsonConvert.DeserializeObject<EmployeeEvaluationResult>(File.ReadAllText(file));
    }

    public static SystemEvaluationrResult LoadSystemEvaluation()
    {
        string file = Path.Combine(EvaluationPath, $"{SYSTEM_EVALUATION_CODE}.json");
        if (!File.Exists(file)) return null;

        return JsonConvert.DeserializeObject<SystemEvaluationrResult>(File.ReadAllText(file));
    }

    public static bool HasSavedEvaluation(string employeeCode)
    {
        string file = Path.Combine(EvaluationPath, $"{employeeCode}.json");
        return File.Exists(file);
    }

    public static void Reset(string employeeCode)
    {
        string file = Path.Combine(EvaluationPath, $"{employeeCode}.json");
        if (File.Exists(file))
            File.Delete(file);
    }

    public static void ResetAll()
    {
        if(Directory.Exists(EvaluationPath)) 
            Directory.Delete(EvaluationPath, true);
    }
}
