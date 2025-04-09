using Common;
using Microsoft.AspNetCore.Mvc;
using Worker.Services;
namespace Worker.Controllers;

public class WorkerController : ControllerBase
{
    private readonly ILogger<WorkerController> _logger;
    private IWorkerService _workerService;

    public WorkerController(
        ILogger<WorkerController> logger,
        IWorkerService workerService)
    {
        _logger = logger;
        _workerService = workerService;
    }

    [HttpPost]
    [Consumes("application/xml")]
    [Route("internal/api/worker/hash/crack/task")]
    public async Task<IActionResult> ProcessTask([FromBody] ManagerCrackRequest request)
    {
        _logger.LogInformation($"start crack hash at {request.RequestId}");
        var task = Task.Run(() => _workerService.Crack(request));

        return await Task.Run(Ok);
    }
}
