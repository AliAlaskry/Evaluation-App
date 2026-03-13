using Newtonsoft.Json;

public class Employee
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsTeamLead { get; set; } = false;

    public bool Include { get; set; } = true;

    [JsonIgnore]
    public string Title => $"{Name} ({Code})";
}