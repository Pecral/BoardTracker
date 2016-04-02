using System;

namespace BoardTracker.Model
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
