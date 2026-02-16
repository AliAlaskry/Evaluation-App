public class Question
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double Weight { get; set; } = 0;
    public bool ManagerOnly { get; set; } = false;
    public bool Include { get; set; } = true;
    public double Min { get; set; } = 0;
    public double Max { get; set; } = 100;
    public double Default { get; set; } = 0;
    public string MinMeaning { get; set; } = string.Empty;
    public string MaxMeaning { get; set; } = string.Empty;
    public double Score { get; set; } = 0;
}
