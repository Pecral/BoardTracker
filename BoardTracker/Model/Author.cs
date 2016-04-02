using System.Xml.Serialization;

namespace BoardTracker.Model
{
    public class Author
    {
        [XmlAttribute]
        public int AuthorId;

        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Lastname;

        [XmlAttribute]
        public string Website;

        [XmlAttribute]
        public string Description;
    }
}
