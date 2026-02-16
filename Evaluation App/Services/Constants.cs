public static class Constants
{
    public static readonly string BasePath = 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Evaluations");

    public static readonly string EMPLOYEE_FILE = 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employees.json");

    public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public const string SYSTEM_EVALUATION_CODE = "SYS_EVAL";
}

