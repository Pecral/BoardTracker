using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DevTracker.Model
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
