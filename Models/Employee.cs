internal class Employee
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsTeamLead { get; set; } = false;

    public bool Include { get; set; } = true;

    public string Title => $"{Name} ({Code})";
}