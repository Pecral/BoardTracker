using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using BoardTracker.BusinessLogic.DataProvider;
using BoardTracker.BusinessLogic.Tracker;
using BoardTracker.Configuration.Model;
using BoardTracker.Repository;
using NLog;

namespace BoardTracker
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                logger.Info("Initialize config..");
                List<TrackingConfiguration> trackingConfigurations = ConfigurationManager.GetSection("trackingConfigurations") as List<TrackingConfiguration>;        

                //parse connection type enum
                RepositoryType connectionType = (RepositoryType)Enum.Parse(typeof(RepositoryType), ConfigurationManager.ConnectionStrings[1].Name, true);
                //get connection string
                var connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
                logger.Info("Database Type: " + connectionType + "; Connection String: " + connectionString);

                logger.Info($"################ Tracked sites ############{Environment.NewLine}");

                //print config parameters
                foreach (var config in trackingConfigurations)
                {
                    logger.Info("###################################################");
                    logger.Info($"Tracking configuration for website {config.WebsiteConfiguration.Website}");
                    logger.Info($"Website: {config.WebsiteConfiguration.Website}");
                    logger.Info($"Website URL: {config.WebsiteConfiguration.WebsiteUrl}");
                    logger.Info($"TrackerType: {config.WebsiteConfiguration.DataProviderType}");
                    logger.Info($"Save Post Content: {config.WebsiteConfiguration.SavePostContent}");
                    logger.Info($"Check posts of the last {config.WebsiteConfiguration.CheckLastXDaysForUpdates} days for updates");
                    logger.Info($"Post History Template URL: {config.WebsiteConfiguration.PostHistoryTemplate}");
                    logger.Info($"Pagination type: {config.WebsiteConfiguration.PaginationType}{Environment.NewLine}");

                    logger.Info($"Request rate in minutes: {config.WebsiteConfiguration.RequestRateConfiguration.RequestRateInMinutes}");
                    logger.Info($"Request timeout in milliseconds: {config.WebsiteConfiguration.RequestRateConfiguration.RequestSleepInMilliseconds}");
                    logger.Info($"Requests till sleep: {config.WebsiteConfiguration.RequestRateConfiguration.RequestsTillSleep}{Environment.NewLine}");

                    logger.Info($"Tracked profiles...");
                    foreach (var profile in config.TrackedProfiles)
                    {
                        logger.Info($"Name: {profile.Name} Template Value: {profile.TemplateKey}");
                    }
                }

                logger.Info($"##########################################{Environment.NewLine}");

                //Start the tracker
                StartTracker(trackingConfigurations, connectionType, connectionString);
            }
            catch (Exception e)
            {
                logger.Error("Critical Error! Exception Message = " + e.Message + "; Inner Message = " + e.InnerException?.Message);
            }

            Console.In.ReadLine();
        }

        /// <summary>
        /// Start the tracking process for the configurations with parallelized ForEach
        /// </summary>
        /// <param name="configs">All tracking profiles</param>
        /// <param name="connectionType">The repository type of the database</param>
        /// <param name="connectionString">The connection string to the database</param>
        public static void StartTracker(List<TrackingConfiguration> configs, RepositoryType connectionType, string connectionString)
        {
            logger.Warn("The tracking will start now!");
            //var c = configs[0];
            Parallel.ForEach(configs, c =>
            {
                try
                {
                    logger.Info($"Start tracking process for {c.WebsiteConfiguration.Website}({c.WebsiteConfiguration.WebsiteUrl})");

                    //create repository
                    ITrackingRepository repo = RepositoryHandler.GetTrackingRepository(connectionType, connectionString);
                    logger.Info($"{c.WebsiteConfiguration.Website} - Database successfully initialized");

                    //create data provider
                    IDataProvider provider = DataProviderHandler.GetProvider(c.WebsiteConfiguration.DataProviderType);

                    //create and start the tracker
                    Tracker tracker = new Tracker(repo, provider, c);
                    tracker.StartTracking();
                }
                catch (Exception e)
                {
                    logger.Error($"Tracking process for website {c.WebsiteConfiguration.Website} cancelled - Exception Message = " + e.Message + "; Inner Message = " + e.InnerException?.Message);
                }
            });
        }
    }
}
