using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTracker.BusinessLogic;
using DevTracker.BusinessLogic.DataProvider;
using DevTracker.BusinessLogic.Selector;

namespace DevTracker.Configuration.Model
{
    public class WebsiteConfiguration
    {
        /// <summary>
        /// The website that we want to track
        /// </summary>
        public string Website;

        /// <summary>
        /// The main url of the website
        /// </summary>
        public string WebsiteUrl;

        /// <summary>
        /// A boolean to specify whether the content of a post should be stored
        /// </summary>
        public bool SavePostContent = true;

        /// <summary>
        /// If a profile is tracked for the first time, the tracker will save the whole post-history.
        /// In the next tracking-rotation, only new posts will be tracked - the program doesn't search for updated old posts if you don't use this option!
        /// If the value is set to, all posts of the last thirty days will be checked for updates.
        /// </summary>
        public int CheckLastXDaysForUpdates = 0;

        /// <summary>
        /// Set the tracker which is used to get the data
        /// </summary>
        public DataProviderType DataProviderType;

        /// <summary>
        /// The url template - its keywords will be used to iterate over the history
        /// Keywords: {profile} will be replaced with the value of the dictionary in "tracked profiles"
        /// {page} will be replaced with the current page if the pagination type is set to UrlTemplate
        /// </summary>
        public string PostHistoryTemplate;

        /// <summary>
        /// A list of content selectors containing jquery selectors which will specify where the content, thread title, thread link etc. of a post is.
        /// </summary>
        public List<ContentSelector> ContentSelectors { get; set; }

        /// <summary>
        /// The pagionation element that will be used to iterate over the history
        /// </summary>
        public PaginationType PaginationType;

        public RequestRateConfiguration RequestRateConfiguration { get; set; }
    }
}
