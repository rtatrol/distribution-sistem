using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Common;

using Microsoft.Extensions.Options;
using Manager.Services;

namespace Manager.Controllers;

[ApiController]
public class ManagerController : ControllerBase
{
    private ILogger<ManagerController> _logger;

    private ITimeoutService _timeoutService;
    private IWorkerTaskService _workerTaskService;

    private readonly int _timeoutInSeconds;

    public ManagerController(
        ILogger<ManagerController> logger,
        ITimeoutService timeoutService,
        IWorkerTaskService workerTaskService,
        IOptions<WorkerOptions> options)
    {
        _logger = logger;
        _timeoutService = timeoutService;
        _workerTaskService = workerTaskService;

        _timeoutInSeconds = options.Value.TimeoutInSeconds;
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/hash/crack")]
    public async Task<CrackResponse> CrackHash([FromBody] CrackRequest request)
    {
        _logger.LogInformation($"Starting crack hash {request.Hash}");

        var requestId = await _workerTaskService.CrackAsync(request);

        Task task = Task.Run(() => _timeoutService.SetTimeoutAsync(requestId, _timeoutInSeconds));

        return new CrackResponse { RequestId = requestId };
    }

    [HttpGet]
    [Route("/api/hash/status")]
    public StatusResponse GetStatus([FromQuery] string requestId)
    {
        _logger.LogInformation($"try get status at requestId = {requestId}");
        return _workerTaskService.GetTask(requestId);
    }

    [HttpPatch]
    [Consumes("application/xml")]
    [Route("/internal/api/manager/hash/crack/request")]
    public void ReceiveWorkerAnswer([FromBody] WorkerAnswerResponse response)
    {
        _logger.LogInformation($"got answer for requestId = {response.RequestId}, from worker part = {response.PartNumber}");
        _workerTaskService.AddPart(response);
    }

}
