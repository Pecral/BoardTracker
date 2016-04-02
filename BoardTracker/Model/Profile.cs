using System.Xml.Serialization;

namespace BoardTracker.Model
{
    public class Profile
    {
        [XmlAttribute]
        public int ProfileId { get; set; }

        [XmlAttribute]
        public string UniqueCode { get; set; }

        [XmlAttribute]
        public int? AuthorId { get; set; }

        [XmlAttribute]
        public int WebsiteId { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string TemplateKey { get; set; }

        [XmlAttribute]
        public string ProfileUrl { get; set; }

    }
}
