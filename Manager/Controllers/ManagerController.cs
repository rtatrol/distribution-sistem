using System.Collections.Concurrent;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Common;
using System.Text;

namespace Manager.Controllers;

[ApiController]
public class ManagerController : ControllerBase
{
    private static Dictionary<String, StatusResponse> _requests = new();
    private static ConcurrentDictionary<String, WorkerTask> workerAnswers = new();
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<ManagerController> _logger;

    private readonly int _maxParts = 1;

    public ManagerController(IHttpClientFactory clientFactory, ILogger<ManagerController> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/hash/crack")]
    public async Task<CrackResponse> CrackHash([FromBody] CrackRequest request)
    {
        _logger.LogInformation($"Starting crack hash {request.Hash}");

        var requestId = Guid.NewGuid().ToString();
        _requests[requestId] = new StatusResponse { Status = RequestState.IN_PROGRESS.ToString() };
        workerAnswers.TryAdd(requestId, new WorkerTask { RequestId = requestId, ExpectedPartCount = _maxParts });//  через IOptions

        var crackRequest = new ManagerCrackRequest
        {
            RequestId = requestId,
            PartNumber = 0,
            PartCount = _maxParts,//вынести в настройки это число
            Hash = request.Hash,
            MaxLength = request.MaxLenght,
            Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789"
        };

        var xmlContent = XmlSerializeService.Serialize(crackRequest);
        var content = new StringContent(xmlContent, Encoding.Unicode, "application/xml");

        var client = _clientFactory.CreateClient();
        await client.PostAsync("http://worker:8080/internal/api/worker/hash/crack/task", content);

        return new CrackResponse { RequestId = requestId };
    }

    [HttpGet]
    [Route("/api/hash/status")]
    public StatusResponse GetStatus([FromQuery] string requestId)
    {
        _logger.LogInformation($"try get status at requestId = {requestId}");
        if (!_requests.TryGetValue(requestId, out var status))
        {
            _logger.LogError($"not found status at requestId = {requestId}");
            return new StatusResponse { Status = RequestState.ERROR.ToString(), Data = null };
        }
        _logger.LogInformation($"succesfully get status at requestId = {requestId}");
        return new StatusResponse { Status = status.Status, Data = status.Data };
    }

    [HttpPatch]
    [Consumes("application/xml")]
    [Route("/internal/api/manager/hash/crack/request")]
    public void ReceiveWorkerAnswer([FromBody] WorkerAnswerResponse response)
    {
        _logger.LogInformation($"got answer for requestId = {response.RequestId}, from worker part = {response.PartNumber}");

        if (workerAnswers.TryGetValue(response.RequestId, out var task))
        {
            task.Responses.Add(response);
            task.ReceivedPartCount++;
            if (task.ReceivedPartCount == task.ExpectedPartCount)
            {
                _logger.LogInformation($"try to set ready status for requestId = {response.RequestId}");

                var result = new List<string>();
                foreach (var resp in task.Responses)
                {
                    result.AddRange(resp.Answers!);
                }
                _requests[response.RequestId].Data = result;
                _requests[response.RequestId].Status = RequestState.READY.ToString();
            }
        }
    }

}
