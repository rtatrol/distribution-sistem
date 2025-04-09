
public class WorkerOptions
{
    public int WorkerCount { get; set; } = int.TryParse(
            Environment.GetEnvironmentVariable("WORKER_COUNT"), out var count) ? count : 1;
    public int TimeoutInSeconds { get; set; } = 10;
    public List<string> Alphabet { get; set; }
        = "abcdefghijklmnopqrstuvwxyz0123456789".Select(ch => ch.ToString()).ToList();
}
