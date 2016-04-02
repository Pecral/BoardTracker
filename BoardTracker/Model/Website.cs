using System.Xml.Serialization;

namespace BoardTracker.Model
{
    public class Website
    {
        [XmlAttribute(AttributeName = "websiteId")]
        public int WebsiteId { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
    }
}
