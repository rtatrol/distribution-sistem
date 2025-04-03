
using System.Security.Cryptography;
using System.Text;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace Worker.Controllers;

public class WorkerController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<WorkerController> _logger;
    public WorkerController(IHttpClientFactory clientFactory, ILogger<WorkerController> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("application/xml")]
    [Route("internal/api/worker/hash/crack/task")]
    public async Task<IActionResult> ProcessTask([FromBody] ManagerCrackRequest request)
    {
        var result = new List<string>();
        var alphabet = request.Alphabet.ToCharArray();

        foreach (var word in GenerateWords(alphabet, request.MaxLength, request.PartNumber, request.PartCount))
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(word));
            if (BitConverter.ToString(hash).Replace("-", "").ToLower() == request.Hash)
            {
                result.Add(word);
            }
        }

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

        return Ok();
    }

    private IEnumerable<string> GenerateWords(char[] alphabet, int maxLength, int partNumber, int partCount)
    {
        for (int length = 1; length <= maxLength; length++)
        {
            long total = (long)Math.Pow(alphabet.Length, length);
            long partSize = total / partCount;
            long start = partNumber * partSize;
            long end = (partNumber == partCount - 1) ? total : start + partSize;

            for (long i = start; i < end; i++)
            {
                yield return GetWord(alphabet, length, i);
            }
        }
    }

    private string GetWord(char[] alphabet, int length, long index)
    {
        var result = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            index = Math.DivRem(index, alphabet.Length, out long rem);
            result.Insert(0, alphabet[rem]);
        }
        return result.ToString();
    }
}
