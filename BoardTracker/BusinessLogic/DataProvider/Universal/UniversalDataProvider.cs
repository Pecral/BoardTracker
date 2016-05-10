using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using BoardTracker.BusinessLogic.Selector;
using BoardTracker.Configuration.Model;
using BoardTracker.Model;
using CsQuery;

namespace BoardTracker.BusinessLogic.DataProvider.Universal
{
    public class UniversalDataProvider : IDataProvider
    {
        public UniversalDataProvider()
        {
            webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Webclient to download html string
        /// </summary>
        private WebClient webClient;

        #region Configuration Properties

        /// <summary>
        /// The request configuration (requests till sleep, timespan per timeout etc.)
        /// </summary>
        private RequestRateConfiguration requestConfig;

        /// <summary>
        /// Current name of the tracked website
        /// </summary>
        private string currentWebsite;

        /// <summary>
        /// The profile template url
        /// </summary>
        private string postHistoryTemplateUrl;

        /// <summary>
        /// Store the post content or not..
        /// </summary>
        private bool savePostContent;

        private PaginationType paginationType;
        /// <summary>
        /// A content selector which gets the element that is used for the pagination (a button with the link for the next site for example
        /// </summary>
        private ContentSelector paginationElement;

        /// <summary>
        /// The jquery selector which will return the list of posts
        /// </summary>
        private ContentSelector postListSelector;

        /// <summary>
        /// ContentSelectors for the element of a post (title, content etc.)
        /// </summary>
        private ContentSelector postElement_threadTitle;
        private ContentSelector postElement_postingDateTime;
        private ContentSelector postElement_content;
        private ContentSelector postElement_forum;
        private ContentSelector postElement_forumLink;
        private ContentSelector postElement_postLink;

        #endregion Configuration Properties

        #region Current Session Properties

        //we'll track the amount of requests since the last timeout
        private int requestsSinceLastSleep;

        /// <summary>
        /// The template url where the {profile}-template is replaced with the currently tracked profile
        /// </summary>
        private string currentProfileTemplateUrl;

        /// <summary>
        /// If we're using a pagination via template url, we'll save the current page number here
        /// </summary>
        private int currentPage;

        /// <summary>
        /// If we're using a pagination via template url, we'll save the number of the last page here.
        /// This integer is nullable because we have to set this value first - it will remain null if the pagination type is set to "ButtonLink".
        /// </summary>
        private int? lastPage;

        #endregion Current Session Properties

        /// <summary>
        /// Save the ContentSelectors which are used to get specific parts of a page (a list of html elements which represent posts) or of a post(for example its content or thread title)
        /// </summary>
        /// <param name="selectors"></param>
        private void SaveContentSelectors(List<ContentSelector> selectors)
        {
            postElement_content = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostElementContent);
            postElement_forum = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostElementForum);
            postElement_postLink = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostElementPostLink);
            postElement_forumLink = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostElementForumLink);
            postElement_postingDateTime = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostElementPostingDateTime);
            postElement_threadTitle = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostElementThreadTitle);

            paginationElement = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PaginationElement);
            postListSelector = selectors.First(x => x.ContentSelectorTarget == ContentSelectorTarget.PostList);
        }

        /// <summary>
        /// Set the tracking configuration of the website which contains the profiles that you want to track
        /// </summary>
        /// <param name="config">The tracking config</param>
        public void SetWebsiteTrackingConfig(WebsiteConfiguration config)
        {
            this.currentWebsite = config.Website;
            this.postHistoryTemplateUrl = config.PostHistoryTemplate;
            this.paginationType = config.PaginationType;
            this.savePostContent = config.SavePostContent;

            this.requestConfig = config.RequestRateConfiguration;

            SaveContentSelectors(config.ContentSelectors);
        }

        /// <summary>
        /// Analyze the whole profile's post history and yield return its posts.
        /// </summary>
        /// <param name="profile">The profile whos post history will be analyzed</param>
        /// <returns></returns>
        public IEnumerable<Post> ProvidePostsInProfile(Profile profile)
        {
            Console.WriteLine($"{Program.DatePattern()} - Start tracking profile {profile.Name} ..");
            List<Post> savedPostsInCurrentSession = new List<Post>();
            List<string> visitedLinksInCurrentSession = new List<string>();

            string currentWebsiteUrl = "";
            string currentWebsiteHtml = "";
            currentPage = 0;
            lastPage = null;

            currentProfileTemplateUrl = postHistoryTemplateUrl.Replace("{profile}", profile.TemplateKey);
            currentWebsiteUrl = currentProfileTemplateUrl.Replace("{page}", currentPage.ToString());

            //iterate through pages - only new sites are visited
            //we'll start with the first site and test whether a next site exists then
            do
            {
                //check whether we have to sleep a little bit so that we don't ddos the server
                CheckTimeout();
                //we'll save the visited links here because we don't want to visit a page multiple times
                visitedLinksInCurrentSession.Add(currentWebsiteUrl);

                currentWebsiteHtml = DownloadHtml(currentWebsiteUrl);

                //break data-provider if we didn't receive any url
                if (currentWebsiteHtml == null)
                {
                    Debug.Write($"{Program.DatePattern()} - Cancelled data-provider for website {currentWebsite} because we were unable to download the html data from the URL ({currentWebsiteUrl})");
                    yield break;
                }
                requestsSinceLastSleep++;

                //abort the import process for this author if we reached posts which are too old
                IEnumerable<Post> postsInPage = ProvidePostsInPage(profile, currentWebsiteHtml, savedPostsInCurrentSession);

                //yield return the posts
                foreach (var p in postsInPage)
                {
                    yield return p;
                }

            } while ((currentWebsiteUrl = RequestNextPageUrl(currentWebsiteHtml)) != null &&
                     visitedLinksInCurrentSession.All(x => x != currentWebsiteUrl));

            Console.WriteLine($"{Program.DatePattern()} - Finished data providing for profile {profile.Name}");
        }

        /// <summary>
        /// Save the post of a user which are displayed in the current html page
        /// </summary>
        /// <param name="profile">The author of the posts</param>
        /// <param name="pageHtml">The html data of the current page</param>
        /// <param name="savedPostsInCurrentSession">The posts we did save in the current session, we don't want to insert posts multiple times</param>
        /// <returns>Returns the models of the post which are displayed in the current page</returns>
        public IEnumerable<Post> ProvidePostsInPage(Profile profile, string pageHtml, List<Post> savedPostsInCurrentSession)
        {
            var dom = CQ.CreateDocument(pageHtml);

            var posts = dom[postListSelector.JqSelector].ToList();
            foreach (var post in posts)
            {
                //get dom of inner html - if we use the post object itself, it'll always be the first one of the list (??)
                var postDom = post.InnerHTML;
                var dateTimeData = SelectorProcessor.ProcessContentSelector(postElement_postingDateTime, postDom);
                DateTime postingDateTime;

                //try to parse the date
                try
                {
                    postingDateTime = DateTime.ParseExact(dateTimeData, postElement_postingDateTime.DateTimeFormat, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{Program.DatePattern()} - {currentWebsite} - The parsing of the post's date failed. The data '{dateTimeData}' does not match the format '{postElement_postingDateTime.DateTimeFormat}'");
                    throw e;
                }

                //trim whitespaces
                string content = savePostContent ? SelectorProcessor.ProcessContentSelector(postElement_content, postDom).Trim() : null;
                string forum = SelectorProcessor.ProcessContentSelector(postElement_forum, postDom);
                string forumLink = SelectorProcessor.ProcessContentSelector(postElement_forumLink, postDom);
                string link = SelectorProcessor.ProcessContentSelector(postElement_postLink, postDom);
                string threadTitle = SelectorProcessor.ProcessContentSelector(postElement_threadTitle, postDom);

                Post model = new Post()
                {
                    ProfileId = profile.ProfileId,
                    Content = content,
                    PostingDateTime = postingDateTime,
                    Forum = forum,
                    ForumLink = forumLink,
                    PostLink = link,
                    ThreadTitle = threadTitle
                };

                //we don't want to return posts multiple times
                if (savedPostsInCurrentSession.All(x => x.PostLink != model.PostLink))
                {
                    savedPostsInCurrentSession.Add(model);

                    yield return model;
                }
            }

            //break import for the current page if something went wrong -- Removable I guess, even for the nested yield?
            //yield break;
        }

        /// <summary>
        /// Returns the url of the next page depending on the current pagination type
        /// </summary>
        /// <param name="currentHtml"></param>
        /// <param name="currentPage"></param>
        /// <param name="maxPage"></param>
        /// <returns></returns>
        private string RequestNextPageUrl(string currentHtml)
        {
            switch (paginationType)
            {
                case PaginationType.ButtonLink:
                    return SelectorProcessor.ProcessContentSelector(paginationElement, currentHtml);

                case PaginationType.TemplateUrl:
                    //search the number of the last page if it's not set
                    if (!lastPage.HasValue)
                    {
                        CQ dom = currentHtml;
                        lastPage = Int32.Parse(SelectorProcessor.ProcessContentSelector(paginationElement, dom) ?? "-1");
                    }

                    if (currentPage < lastPage.Value)
                    {
                        //increase current page
                        currentPage++;

                        //replace the page-template with the current page and return the url
                        return currentProfileTemplateUrl.Replace("{page}", currentPage.ToString());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        /// <summary>
        /// Check whether we have to take a nap
        /// </summary>
        private void CheckTimeout()
        {
            if (requestsSinceLastSleep >= requestConfig.RequestsTillSleep)
            {
                Console.WriteLine($"{Program.DatePattern()} - {currentWebsite} - Amount of requests till the last sleep is too much, we'll take a nap for now");
                Thread.Sleep(TimeSpan.FromMilliseconds(requestConfig.RequestSleepInMilliseconds));
                Console.WriteLine($"{Program.DatePattern()} - {currentWebsite} - We're back!");

                requestsSinceLastSleep = 0;
            }
            else if(requestConfig.RequestDelayInMilliseconds > 0)
            {
                //request delay..
                Thread.Sleep(TimeSpan.FromMilliseconds(requestConfig.RequestDelayInMilliseconds));
            }
        }

        /// <summary>
        /// Return the html data of a website..
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string DownloadHtml(string url)
        {
            int tries = 0;

            //Try it 3 times to prevent timeout issues..
            while (tries < 3)
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    string html = webClient.DownloadString(url);
                    return html;
                }
                catch (Exception)
                {
                    Console.WriteLine($"{Program.DatePattern()} - {currentWebsite} - Failed to download html after {watch.Elapsed.TotalSeconds} seconds - {tries}. try");
                    tries++;
                }
                watch.Stop();
            }

            return null;
        }
    }
}
