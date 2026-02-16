public class Employee
{
    public string Code { get; set; } = string.Empty;       // الكود الوظيفي
    public string Name { get; set; } = string.Empty;       // اسم الموظف
    public bool IsTeamLead { get; set; } = false;          // هل الموظف قائد فريق

    // التحكم في المشاركة داخل عملية التقييم من ملف JSON بسهولة
    public bool Include { get; set; } = true;

    public Employee()
    {
    }

    public Employee(string code, string name, bool isTeamLead)
    {
        Code = code;
        Name = name;
        IsTeamLead = isTeamLead;
    }
}
