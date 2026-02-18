using Evaluation_App.Services;
using Newtonsoft.Json;
using static Constants;

public static class EmployeeService
{
    private static List<Employee> allEmployees;
    private static List<Employee> otherEmployees;

    public static List<Employee> AllEmployees
    {
        get
        {
            allEmployees ??= new();
            if (!allEmployees.Any())
                Initialize();

            return allEmployees;
        }
    }
    public static List<Employee> OtherEmployees
    {
        get
        {
            otherEmployees ??= new();
            if (!otherEmployees.Any())
                SetOtherEmployees();

            return otherEmployees;
        }
    }

    private static void Initialize()
    {
        LoadEmployees();
        SetOtherEmployees();
    }
    private static void SetOtherEmployees()
    {
        if (AuthService.CurrentUser == null)
            return;

        allEmployees ??= new();
        otherEmployees = allEmployees.Where(e => !e.Code.Equals(AuthService.CurrentUser.Code)).ToList();
    }

    private static void LoadEmployees()
    {
        if (allEmployees != null && allEmployees.Count != 0)
            return;

        try
        {
            if (!File.Exists(EMPLOYEE_FILE))
            {
                MessageBox.Show("ملف الموظفين غير موجود:\n" + EMPLOYEE_FILE, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                allEmployees = new();
                return;
            }

            string json = File.ReadAllText(EMPLOYEE_FILE);
            allEmployees = JsonConvert.DeserializeObject<List<Employee>>(json);

            if (allEmployees == null)
                allEmployees = new();
            else
                allEmployees = allEmployees.Where(e => e.Include).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف الموظفين:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            allEmployees = new List<Employee>();
        }
    }

    public static Employee GetEmployeeByCode(string code)
    {
        return AllEmployees.FirstOrDefault(e => e.Code == code);
    }
}
