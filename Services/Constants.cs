public static class Constants
{
    public static readonly string EvaluationPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Evaluations");

    public static readonly string EMPLOYEE_FILE =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employees.json");

    public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public const string ExcelPostfix = ".xlsx";
    public const string EvaluationSperator = "Evaulated";

    public const string SYSTEM_EVALUATION_CODE = "System";

    public const string TEAM_MEMBERS_REPORT_FILE_NAME = "Team Members";
    public const string FULL_SURVEY_FILE_NAME = "Full Servey";
    public const string SPRINT_FULL_SURVEY_FILE_NAME = "Sprint Full Survey";

    public const string WORKSHEET_SYSTEM_TITLE = "System Evaluation";
    public const string WORKSHEET_EMPLOYEE_TITLE_PREFIX = "Employee Evaluation - ";

    public const string POSTFIX_ENTITY_TOTAL = " - Total";
    public const string LABEL_TOTAL = "Total";
    public const string LABEL_NOTES = "Suggestions / Notes / General";
    public const string LABEL_TEAM_LEADER_ASSISTANT_READY = "Ready to be team leader assistant";
    public const string LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION = "Recommend as team leader assistant";
    public const string LABEL_TEAM_LEADER_ASSISTANT_YES = "Yes";
    public const string LABEL_TEAM_LEADER_ASSISTANT_NO = "No";

    public const int ROW_HEADER_VISIBLE = 1;
    public const int ROW_EVALUATOR_META = 2; 

    public const int COLUMN_ID = 1;
    public const int COLUMN_LABEL = 2;
    public const int COLUMN_MEANING= 3;
    public const int COLUMN_START_VALUE = 4;
}
