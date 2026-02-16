using Evaluation_App.Services;
using Newtonsoft.Json;
using static Constants;

public static class EmployeeService
{
    private static List<Employee> employees;

    public static List<Employee> LoadEmployees()
    {
        if (employees != null)
            return employees;

        try
        {
            if (!File.Exists(EMPLOYEE_FILE))
            {
                MessageBox.Show("ملف الموظفين غير موجود:\n" + EMPLOYEE_FILE, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new();
            }

            string json = File.ReadAllText(EMPLOYEE_FILE);
            employees = JsonConvert.DeserializeObject<List<Employee>>(json);

            if (employees == null)
                employees = new();

            return employees.Where(e => e.Include).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ أثناء تحميل ملف الموظفين:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<Employee>();
        }
    }

    public static Employee GetEmployeeByCode(string code)
    {
        return LoadEmployees().FirstOrDefault(e => e.Code == code);
    }
}
