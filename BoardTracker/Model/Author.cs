using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DevTracker.Model
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
