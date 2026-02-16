using Newtonsoft.Json;
using static Constants;

public static class EvaluationService
{
    public static void Save(EvaluationResult eval)
    {
        if (!Directory.Exists(EvaluationPath))
            Directory.CreateDirectory(EvaluationPath);

        string file = Path.Combine(EvaluationPath, $"{eval.Code}.json");
        File.WriteAllText(file, JsonConvert.SerializeObject(eval, Formatting.Indented));
    }

    public static EvaluationResult LoadEvaluation(string employeeCode)
    {
        string file = Path.Combine(EvaluationPath, $"{employeeCode}.json");
        if (!File.Exists(file)) return null;

        return JsonConvert.DeserializeObject<EvaluationResult>(File.ReadAllText(file));
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
