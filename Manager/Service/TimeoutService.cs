using System.Threading.Tasks;
using Manager.Models;
namespace Manager.Services;
public interface ITimeoutService
{
    public Task SetTimeoutAsync(string requestId, int timeoutInSeconds);
}

public class TimeoutService : ITimeoutService
{
    private ILogger<TimeoutService> _logger;
    private IWorkerTaskService _workerTaskService;

    public TimeoutService(
        ILogger<TimeoutService> logger,
        IWorkerTaskService workerTaskService)
    {
        _logger = logger;
        _workerTaskService = workerTaskService;
    }

    public async Task SetTimeoutAsync(string requestId, int timeoutInSeconds)
    {
        await Task.Delay(timeoutInSeconds * 1000);

        var task = _workerTaskService.GetTask(requestId);

        if (task.Status.ToEnum() == RequestState.IN_PROGRESS)
        {
            _workerTaskService.UpdateTaskStatus(requestId, RequestState.ERROR);
            _logger.LogInformation("request " + requestId + " is timed out");
        }
    }
}
