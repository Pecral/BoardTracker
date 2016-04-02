using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DevTracker.Model
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
