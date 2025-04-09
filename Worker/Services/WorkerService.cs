using Common;
using System.Security.Cryptography;
using System.Text;
namespace Worker.Services;

public interface IWorkerService
{
    public Task Crack(ManagerCrackRequest request);
}
public class WorkerService : IWorkerService
{
    private ILogger<WorkerService> _logger;
    private IHttpClientFactory _clientFactory;
    public WorkerService(
        ILogger<WorkerService> logger,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task Crack(ManagerCrackRequest request)
    {
        var result = new List<string>();
        var alphabet = request.Alphabet;

        foreach (var word in GenerateWords(alphabet, request.MaxLength, request.PartNumber, request.PartCount))
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(word));
            if (BitConverter.ToString(hash).Replace("-", "").ToLower() == request.Hash)
            {
                result.Add(word);
            }
        }
        _logger.LogInformation($"complete crack on part number {request.PartNumber}");

        var response = new WorkerAnswerResponse
        {
            RequestId = request.RequestId,
            PartNumber = request.PartNumber,
            Answers = result
        };

        var xmlContent = XmlSerializeService.Serialize(response);
        var content = new StringContent(xmlContent, Encoding.Unicode, "application/xml");

        var client = _clientFactory.CreateClient();
        await client.PatchAsync("http://manager:8080/internal/api/manager/hash/crack/request", content);
    }

    private IEnumerable<string> GenerateWords(List<string> alphabet, int maxLength, int partNumber, int partCount)
    {
        for (int length = 1; length <= maxLength; length++)
        {
            long total = (long)Math.Pow(alphabet.Count, length);
            long partSize = total / partCount;
            long start = partNumber * partSize;
            long end = (partNumber == partCount - 1) ? total : start + partSize;

            for (long i = start; i < end; i++)
            {
                yield return GetWord(alphabet, length, i);
            }
        }
    }

    private string GetWord(List<string> alphabet, int length, long index)
    {
        var result = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            index = Math.DivRem(index, alphabet.Count, out long rem);
            result.Insert(0, alphabet[(int)rem]);
        }
        return result.ToString();
    }
}
