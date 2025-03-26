
using System.Xml.Serialization;
namespace Common;

[XmlRoot]
public class WorkerAnswerResponse
{
    [XmlElement("RequestId")]
    public string RequestId { get; set; } = "";
    [XmlElement("PartNumber")]
    public int PartNumber { get; set; }

    [XmlArray("Answers")]
    [XmlArrayItem("words")]
    public List<string>? Answers { get; set; }
}
