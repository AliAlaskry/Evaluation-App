public class Employee
{
    public string Code { get; set; }       // الكود الوظيفي
    public string Name { get; set; }       // اسم الموظف
    public bool IsTeamLead { get; set; } = false;   // هل الموظف قائد فريق
    public bool Include { get; set; } = true;

    public Employee(string code, string name, bool isTeamLead)
    {
        Code = code;
        Name = name;
        IsTeamLead = isTeamLead;
    }
}
