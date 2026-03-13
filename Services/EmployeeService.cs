using Newtonsoft.Json.Linq;
using static Constants;

internal static class EmployeeService
{
    public static List<Employee> AllEmployees { get; private set; } = new();
    public static List<Employee> OtherEmployees { get; private set; } = new();

    public static void Initialize()
    {
        LoadEmployees();
    }
    public static void SetOtherEmployees()
    {
        if (AuthService.CurrentUser == null)
            return;

        AllEmployees ??= new();
        OtherEmployees = AllEmployees.Where(e => !e.Code.Equals(AuthService.CurrentUser.Code)).ToList();
    }

    public static void ClearOtherEmployees()
    {
        OtherEmployees ??= new();
        OtherEmployees.Clear();
    }

    private static void LoadEmployees()
    {
        if (AllEmployees != null && AllEmployees.Count != 0)
            return;

        try
        {
            if (!File.Exists(EMPLOYEE_FILE))
            {
                MessageBox.Show("ملف الموظفين غير موجود:\n" + EMPLOYEE_FILE, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AllEmployees = new();
                return;
            }

            string json = File.ReadAllText(EMPLOYEE_FILE);
            JObject obj = JObject.Parse(json);
            AllEmployees = obj.SelectToken("Employees").ToObject<List<Employee>>();

            if (AllEmployees == null)
                AllEmployees = new();
            else
                AllEmployees = AllEmployees.Where(e => e.Include).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف الموظفين:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AllEmployees = new List<Employee>();
        }
    }

    public static Employee? GetEmployeeByCode(string code)
    {
        return AllEmployees.FirstOrDefault(e => e.Code.Equals(code));
    }
}
