
using System.Xml.Serialization;

namespace Common;
[XmlRoot]
public class ManagerCrackRequest
{
    [XmlElement("RequestId")]
    public string RequestId { get; set; } = "";

    [XmlElement("PartNumber")]
    public int PartNumber { get; set; }

    [XmlElement("PartCount")]
    public int PartCount { get; set; }

    [XmlElement("Hash")]
    public string Hash { get; set; } = "";

    [XmlElement("MaxLength")]
    public int MaxLength { get; set; }

    [XmlArray("Alphabet")]
    [XmlArrayItem("symbols")]
    public List<string> Alphabet { get; set; } = new();
}
