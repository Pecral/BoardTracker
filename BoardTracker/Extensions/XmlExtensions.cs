using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BoardTracker.Extensions
{
    public static class XmlExtensions
    {
        public static XElement ToXElement<T>(this object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    xmlSerializer.Serialize(streamWriter, obj, ns);
                    XElement parsed = XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray()));
                    return parsed;
                }
            }
        }

        public static T FromXElement<T>(this XElement xElement)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(xElement.CreateReader());
        }
    }
}
