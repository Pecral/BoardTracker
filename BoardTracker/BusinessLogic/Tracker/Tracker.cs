using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BoardTracker.BusinessLogic.DataProvider;
using BoardTracker.Configuration.Model;
using BoardTracker.Model;
using BoardTracker.Repository;

namespace BoardTracker.BusinessLogic.Tracker
{
    public class Tracker
    {

        /// <summary>
        /// Creates a universal tracker to track forum posts.
        /// This instance will be used for only one website at a time.
        /// </summary>
        /// <param name="repo">The repository which will be used to store the data</param>
        /// <param name="config">The configuration parameters for the tracking process</param>
        public Tracker(ITrackingRepository repo, IDataProvider dataProvider, TrackingConfiguration config)
        {
            this.repository = repo;
            this.dataProvider = dataProvider;
            this.requestConfig = config.WebsiteConfiguration.RequestRateConfiguration;
            this.trackedProfiles = config.TrackedProfiles;
            this.checkPostsOfLastXDaysForUpdates = config.WebsiteConfiguration.CheckLastXDaysForUpdates;

            website = new Website()
            {
                Name = config.WebsiteConfiguration.Website,
                Url = config.WebsiteConfiguration.WebsiteUrl
            };

            //upsert models
            UpdateWebsiteAndProfileModels(website, trackedProfiles);

            dataProvider.SetWebsiteTrackingConfig(config.WebsiteConfiguration);
        }

        /// <summary>
        /// current website that is tracked
        /// </summary>
        private Website website;

        /// <summary>
        /// Repository to store the data
        /// </summary>
        private ITrackingRepository repository;

        /// <summary>
        /// The dataprovider which will provide the profile's posts
        /// </summary>
        private IDataProvider dataProvider;

        /// <summary>
        /// The profiles which have to be tracked - Key is the author's name, value is the part which is used in the template
        /// </summary>
        private List<Profile> trackedProfiles;

        /// <summary>
        /// The request configuration (requests till sleep, timespan per timeout etc.)
        /// </summary>
        private RequestRateConfiguration requestConfig;

        /// <summary>
        /// If a profile is tracked for the first time, the tracker will save the whole post-history.
        /// In the next tracking-rotation, only new posts will be tracked - the program doesn't search for updated old posts if you don't use this option!
        /// If the value is set to, all posts of the last thirty days will be checked for updates.
        /// </summary>
        private int checkPostsOfLastXDaysForUpdates;

        /// <summary>
        /// Start trackinf for all profiles
        /// </summary>
        public void StartTracking()
        {
            while (true)
            {
                Console.WriteLine($"{Program.DatePattern()} - {website.Name} - Start tracking..");

                //iterate through the profiles and start the tracking process
                foreach (var profile in trackedProfiles)
                {
                    StartTrackingOfProfile(profile);
                }

                Console.WriteLine($"{Program.DatePattern()} - {website.Name} - Sleep for {requestConfig.RequestRateInMinutes} minutes..");
                Thread.Sleep(new TimeSpan(0, requestConfig.RequestRateInMinutes, 0));
            }
        }
        
        /// <summary>
        /// Start the tracking process for a specific profile
        /// </summary>
        /// <param name="profileName">The name of the author.</param>
        /// <param name="profileTemplateKey">The key which is used to fill the URL</param>
        private void StartTrackingOfProfile(Profile profile)
        {
            //get the datetime of the last post
            DateTime? lastPostTime = GetDateTimeOfLastPost(website, profile) ?? DateTime.MinValue;
            List<Post> postsBatch = new List<Post>();

            DateTime postCheckDateTime = DateTime.Now.AddDays(checkPostsOfLastXDaysForUpdates * (-1));
            Console.WriteLine($"{Program.DatePattern()} - {website.Name} - All posts which are newer than {postCheckDateTime} will be updated");

            //iterate through posts and add new posts
            foreach (Post post in dataProvider.ProvidePostsInProfile(profile))
            {
                //add models if the posts are newer than the stored posts
                if (!lastPostTime.HasValue || post.PostingDateTime.CompareTo(lastPostTime) > 0)
                {
                    postsBatch.Add(post);

                    if (postsBatch.Count >= 50)
                    {
                        try
                        {
                            repository.AddPosts(postsBatch);
                            postsBatch.Clear();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{Program.DatePattern()} - {website.Name} - Post insertion failed - Exception Message = " +
                                            e.Message + "; Inner Message = " +
                                            e.InnerException?.Message);

                            //print lengths for eventual truncation of strings
                            var maxForumLink = postsBatch.OrderBy(x => x.ForumLink.Length).LastOrDefault().ForumLink.Length;
                            var maxForumName = postsBatch.OrderBy(x => x.Forum.Length).LastOrDefault().Forum.Length;
                            var maxContent = postsBatch.OrderBy(x => x.Content.Length).LastOrDefault().Content.Length;
                            var maxPostLink = postsBatch.OrderBy(x => x.PostLink.Length).LastOrDefault().PostLink.Length;
                            var maxThreadTitle = postsBatch.OrderBy(x => x.ThreadTitle.Length).LastOrDefault().ThreadTitle.Length;

                            Console.WriteLine($"{Program.DatePattern()} - {website.Name} - Max Lengths: ForumLink {maxForumLink} - ForumName {maxForumName} - Content {maxContent} - PostLink {maxPostLink} - ThreadTitle {maxThreadTitle}");
                        }
                    }
                }
                //upsert posts if they are newer than the specified check-date
                else if (post.PostingDateTime.CompareTo(postCheckDateTime) >= 0)
                {
                    repository.UpsertPost(post);
                }
                //stop if the posts are too old
                else
                {
                    break;
                }
            }

            //add posts that weren't inserted in the batch
            try
            {
                repository.AddPosts(postsBatch);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Program.DatePattern()} - {website.Name} - Post insertion failed - Exception Message = " +
                                e.Message + "; Inner Message = " +
                                e.InnerException?.Message);
            }
        }

        /// <summary>
        /// get the datetime of the user's last post
        /// we'll use the datetime so that can process only new posts
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private DateTime? GetDateTimeOfLastPost(Website website, Profile profile)
        {
            DateTime? lastPostTime = repository.GetDateOfLastPost(profile);
            if (lastPostTime.HasValue)
            {
                Console.WriteLine($"{Program.DatePattern()} - {website.Name} - DateTime of " + profile.Name + "'s last post: " + lastPostTime.Value.ToString("dd.MM.yyyy hh:mm:ss"));
            }
            else
            {
                Console.WriteLine($"{Program.DatePattern()} - {website.Name} - {profile.Name} does not have a post tracked yet.");
            }

            return lastPostTime;
        }

        /// <summary>
        /// Update the website model and the profile models in the database
        /// We will get the upserted models after the operation so set the model id
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profiles"></param>
        private void UpdateWebsiteAndProfileModels(Website website, List<Profile> profiles)
        {
            repository.UpsertWebsite(website);
            //get model from the database so that our model has the id
            website = repository.GetWebsite(website.Name);

            for (int i = 0; i < profiles.Count; i++)
            {
                profiles[i].WebsiteId = website.WebsiteId;
                repository.UpsertProfile(profiles[i]);

                //get model from the database so that our model has the id
                profiles[i] = repository.GetProfile(profiles[i].TemplateKey);
                
            }
        }
    }
}
