using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DevTracker.Model
{
    public class Post
    {
        public Post()
        {
            PostingDateTime = DateTime.Now.AddYears(-1000);
            Content = "";
            PostLink = "";
            Forum = "";
            ThreadTitle = "";
            ProfileId = -1;
        }

        public int PostId { get; set; }

        public int ProfileId { get; set; }

        public DateTime PostingDateTime { get; set; }

        public string Content { get; set; }

        public string ThreadTitle { get; set; }

        public string PostLink { get; set; }

        public string ForumLink { get; set; }

        public string Forum { get; set; }
    }
}
