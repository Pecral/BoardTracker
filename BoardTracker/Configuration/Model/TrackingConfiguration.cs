using System.Collections.Generic;
using BoardTracker.Model;

namespace BoardTracker.Configuration.Model
{
    public class TrackingConfiguration
    {
        /// <summary>
        /// Tracking configuration of the website (request config, content selectors etc.)
        /// </summary>
        public WebsiteConfiguration WebsiteConfiguration { get; set;}

        /// <summary>
        /// A dictionary of the profiles that you want to track
        /// Key: Name of the user
        /// Value: Value which replaces the {author} keyword in the url template
        /// </summary>
        public List<Profile> TrackedProfiles { get; set; }


    }
}
