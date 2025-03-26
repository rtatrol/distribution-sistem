using Common;
namespace Manager.Models;

public class WorkerTask
{
    public int ExpectedPartCount { get; set; }
    public int ReceivedPartCount { get; set; }
    public HashSet<WorkerAnswerResponse> Responses { get; set; } = new();
}
