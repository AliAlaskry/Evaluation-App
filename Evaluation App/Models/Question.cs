public class Question
{
    public string Id { get; set; }       
    public string Text { get; set; }    
    public double Weight { get; set; } = 0; 
    public bool ManagerOnly { get; set; } = false;
    public double Score { get; set; } = 0;
}