using Newtonsoft.Json;
using static Constants;

internal static class EvaluationService
{
    public static void Save(EvaluationInstance eval)
    {
        SaveEvaluation(eval.FileName, eval);
    }

    private static void SaveEvaluation(string filename, object evaluation)
    {
        if (!Directory.Exists(EvaluationPath))
            Directory.CreateDirectory(EvaluationPath);

        string file = Path.Combine(EvaluationPath, $"{filename}.json");
        File.WriteAllText(file, JsonConvert.SerializeObject(evaluation, Formatting.Indented));
    }

    public static bool TryLoadEvaluation(Employee evaluator, out EvaluationInstance eval,
        Employee? beingEvaluated = null)
    {
        eval = null;

        string filename = ExcelExportService.GetFileName(evaluator, beingEvaluated);

        string file = Path.Combine(EvaluationPath, $"{filename}.json");
        if (!File.Exists(file)) return false;

        eval = JsonConvert.DeserializeObject<EvaluationInstance>(File.ReadAllText(file));
        eval?.PostJsonParsing();
        return true;
    }

    public static void Reset(EvaluationInstance destination)
    {
        string file = Path.Combine(EvaluationPath, $"{destination.FileName}.json");
        if (File.Exists(file))
            File.Delete(file);
    }

    public static void ResetAll()
    {
        if(Directory.Exists(EvaluationPath)) 
            Directory.Delete(EvaluationPath, true);
    }
}
