using System.Collections.Generic;
using BoardTracker.Configuration.Model;
using BoardTracker.Model;

namespace BoardTracker.BusinessLogic.DataProvider
{
    public interface IDataProvider
    {
        /// <summary>
        /// Analyze the whole profile's post history and return its posts.
        /// This should be implemented lazily (with yields or whatever) because some profiles will certainly be pretty big.
        /// </summary>
        /// <param name="profile">The profile whos post history will be analyzed</param>
        /// <returns></returns>
        IEnumerable<Post> ProvidePostsInProfile(Profile profile);

        /// <summary>
        /// Save the post of a user which are displayed in the current html page
        /// </summary>
        /// <param name="profile">The author of the posts</param>
        /// <param name="pageHtml">The html data of the current page</param>
        /// <param name="savedPostsInCurrentSession">The posts we did save in the current session, we don't want to insert posts multiple times</param>
        /// <returns>Returns the models of the post which are displayed in the current page</returns>
        IEnumerable<Post> ProvidePostsInPage(Profile profile, string pageHtml, List<Post> savedPostsInCurrentSession);

        /// <summary>
        /// Set the tracking configuration of the website which contains the profiles that you want to track
        /// </summary>
        /// <param name="config">The tracking config</param>
        void SetWebsiteTrackingConfig(WebsiteConfiguration config);
    }
}
