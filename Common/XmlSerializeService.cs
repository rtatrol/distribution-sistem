using System.Collections.Generic;
using System.Xml.Serialization;
public static class XmlSerializeService
{
    public static string Serialize<T>(T request)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var stringWriter = new StringWriter();
        xmlSerializer.Serialize(stringWriter, request);
        return stringWriter.ToString();
    }

    public static T Deserialize<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stringReader = new StringReader(xml);
        return (T)serializer.Deserialize(stringReader)!;
    }
}
