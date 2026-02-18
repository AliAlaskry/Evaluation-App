using Newtonsoft.Json;
using static Constants;

public static class EvaluationService
{
    public static void Save(EvaluationBase eval)
    {
        SaveEvaluation(eval.Filename, eval);
    }

    private static void SaveEvaluation(string filename, object evaluation)
    {
        if (!Directory.Exists(EvaluationPath))
            Directory.CreateDirectory(EvaluationPath);

        string file = Path.Combine(EvaluationPath, $"{filename}.json");
        File.WriteAllText(file, JsonConvert.SerializeObject(evaluation, Formatting.Indented));
    }

    public static T LoadEvaluation<T>(string filename) where T : EvaluationBase
    {
        string file = Path.Combine(EvaluationPath, $"{filename}.json");
        if (!File.Exists(file)) return null;

        return JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
    }

    public static void Reset(EvaluationBase destination)
    {
        string file = Path.Combine(EvaluationPath, $"{destination.Filename}.json");
        if (File.Exists(file))
            File.Delete(file);
    }

    public static void ResetAll()
    {
        if(Directory.Exists(EvaluationPath)) 
            Directory.Delete(EvaluationPath, true);
    }
}
