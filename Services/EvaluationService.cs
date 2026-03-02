using Newtonsoft.Json;
using static Constants;

internal static class EvaluationService
{
    public static void Save(EvaluationInstance eval)
    {
        SaveEvaluation(eval.FileNameWithExension, eval);
    }

    private static void SaveEvaluation(string filename, object evaluation)
    {
        if (!Directory.Exists(EvaluationPath))
            Directory.CreateDirectory(EvaluationPath);

        string file = Path.Combine(EvaluationPath, $"{filename}.json");
        File.WriteAllText(file, JsonConvert.SerializeObject(evaluation, Formatting.Indented));
    }

    public static EvaluationInstance? LoadEvaluation(string filename)
    {
        string file = Path.Combine(EvaluationPath, $"{filename}.json");
        if (!File.Exists(file)) return null;

        return JsonConvert.DeserializeObject<EvaluationInstance>(File.ReadAllText(file));
    }

    public static void Reset(EvaluationInstance destination)
    {
        string file = Path.Combine(EvaluationPath, $"{destination.FileNameWithExension}.json");
        if (File.Exists(file))
            File.Delete(file);
    }

    public static void ResetAll()
    {
        if(Directory.Exists(EvaluationPath)) 
            Directory.Delete(EvaluationPath, true);
    }
}
