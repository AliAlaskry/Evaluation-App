public static class Constants
{
    public static readonly string EvaluationPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Evaluations");

    public static readonly string EMPLOYEE_FILE =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employees.json");

    public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public const string SYSTEM_EVALUATION_CODE = "SYS_EVAL";

    public const string TEAM_MEMBERS_REPORT_FILE_NAME = "Team Members Report.xlsx";
    public const string SYSTEM_EVALUATION_FILE_NAME = "System Evaluation.xlsx";
    public const string FULL_SURVEY_FILE_NAME = "Full Servey.xlsx";
    public const string SPRINT_FULL_SURVEY_FILE_NAME = "Sprint Full Survey.xlsx";

    public const string WORKSHEET_SYSTEM_TITLE = "System Evaluation";
    public const string WORKSHEET_EMPLOYEE_TITLE_PREFIX = "Employee Evaluation - ";

    public const string LABEL_SECTION_TOTAL = "Section Total";
    public const string LABEL_TOTAL = "Total";
    public const string LABEL_NOTES = "Notes";
    public const string LABEL_TEAM_LEADER_ASSISTANT = "Team leader assistant";
    public const string LABEL_TEAM_LEADER_ASSISTANT_YES = "Yes";

    public const int COLUMN_LABEL = 1;
    public const int COLUMN_VALUE = 2;
}
