using Manager.Models;
using Common;
using System.Text;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Manager.Services;
public interface IWorkerTaskService
{
    public Task<string> CrackAsync(CrackRequest request);
    public StatusResponse GetTask(string requestId);
    public void AddPart(WorkerAnswerResponse response);
    public void UpdateTaskStatus(string requestId, RequestState state);
}
public class WorkerTaskService : IWorkerTaskService
{

    private readonly int _workersCount;
    private readonly List<string> _alphabet;
    private static Dictionary<string, StatusResponse> _requests = new();
    private static ConcurrentDictionary<string, WorkerTask> workerAnswers = new();
    private IHttpClientFactory _clientFactory;


    public WorkerTaskService(IHttpClientFactory clientFactory, IOptions<WorkerOptions> options)
    {
        _workersCount = options.Value.WorkerCount;
        _alphabet = options.Value.Alphabet;

        _clientFactory = clientFactory;

    }
    public async Task<string> CrackAsync(CrackRequest request)
    {
        var requestId = Guid.NewGuid().ToString();
        _requests[requestId] = new StatusResponse { Status = RequestState.IN_PROGRESS.ToString() };
        workerAnswers.TryAdd(requestId, new WorkerTask { RequestId = requestId, ExpectedPartCount = _workersCount });

        for (int i = 0; i < _workersCount; i++)
        {
            var crackRequest = new ManagerCrackRequest
            {
                RequestId = requestId,
                PartNumber = i,
                PartCount = _workersCount,
                Hash = request.Hash,
                MaxLength = request.MaxLenght,
                Alphabet = _alphabet
            };

            var xmlContent = XmlSerializeService.Serialize(crackRequest);
            var content = new StringContent(xmlContent, Encoding.Unicode, "application/xml");

            var client = _clientFactory.CreateClient();
            var task = Task.Run(() => client.PostAsync("http://worker:8080/internal/api/worker/hash/crack/task", content));
        }
        return await Task.Run(() => { return requestId; });
    }

    public StatusResponse GetTask(string requestId)
    {
        if (!_requests.TryGetValue(requestId, out var status))
        {
            return new StatusResponse { Status = RequestState.ERROR.ToString(), Data = null };
        }
        return new StatusResponse { Status = status.Status, Data = status.Data };
    }

    public void AddPart(WorkerAnswerResponse response)
    {
        if (workerAnswers.TryGetValue(response.RequestId, out var task))
        {
            task.Responses.Add(response);
            task.ReceivedPartCount++;
            if (task.ReceivedPartCount == task.ExpectedPartCount)
            {
                task.State = RequestState.READY;
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

    public void UpdateTaskStatus(string requestId, RequestState state)
    {
        if (workerAnswers.TryGetValue(requestId, out var task))
            task.State = state;
        _requests[requestId].Status = state.ToString();
    }


}
