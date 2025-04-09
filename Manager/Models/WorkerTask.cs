using System.Runtime.CompilerServices;
using Common;
namespace Manager.Models;

public class WorkerTask
{
    public string RequestId { get; set; } = "";
    public RequestState State { get; set; } = RequestState.IN_PROGRESS;
    public int ExpectedPartCount { get; set; }
    public int ReceivedPartCount { get; set; } = 0;
    public HashSet<WorkerAnswerResponse> Responses { get; set; } = new();
}
