using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using BoardTracker.BusinessLogic.DataProvider;
using BoardTracker.BusinessLogic.Tracker;
using BoardTracker.Configuration.Model;
using BoardTracker.Repository;

namespace BoardTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initialize config..");
                List<TrackingConfiguration> trackingConfigurations = ConfigurationManager.GetSection("trackingConfigurations") as List<TrackingConfiguration>;        

                //parse connection type enum
                RepositoryType connectionType = (RepositoryType)Enum.Parse(typeof(RepositoryType), ConfigurationManager.ConnectionStrings[1].Name, true);
                //get connection string
                var connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
                Console.WriteLine("Database Type: " + connectionType + "; Connection String: " + connectionString);

                Console.WriteLine("################ Tracked sites ############\n");

                //print config parameters
                foreach (var config in trackingConfigurations)
                {
                    Console.WriteLine("\n###################################################");
                    Console.WriteLine($"Tracking configuration for website {config.WebsiteConfiguration.Website}");
                    Console.WriteLine($"Website: {config.WebsiteConfiguration.Website}");
                    Console.WriteLine($"Website URL: {config.WebsiteConfiguration.WebsiteUrl}");
                    Console.WriteLine($"TrackerType: {config.WebsiteConfiguration.DataProviderType}");

                    Console.WriteLine($"Save Post Content: {config.WebsiteConfiguration.SavePostContent}");
                    Console.WriteLine($"Check posts of the last {config.WebsiteConfiguration.CheckLastXDaysForUpdates} days for updates");

                    Console.WriteLine($"Post History Template URL: {config.WebsiteConfiguration.PostHistoryTemplate}");

                    Console.WriteLine("");
                    Console.WriteLine($"Request rate in minutes: {config.WebsiteConfiguration.RequestRateConfiguration.RequestRateInMinutes}");
                    Console.WriteLine($"Request timeout in milliseconds: {config.WebsiteConfiguration.RequestRateConfiguration.RequestSleepInMilliseconds}");
                    Console.WriteLine($"Requests till sleep: {config.WebsiteConfiguration.RequestRateConfiguration.RequestsTillSleep}");

                    Console.WriteLine($"Pagination type: {config.WebsiteConfiguration.PaginationType}");

                    Console.WriteLine("");
                    Console.WriteLine("Tracked profiles...");
                    foreach (var profile in config.TrackedProfiles)
                    {
                        Console.WriteLine($"Name: {profile.Name} Template Value: {profile.TemplateKey}");
                    }
                }

                Console.WriteLine("\n##########################################");

                //Start the tracker
                StartTracker(trackingConfigurations, connectionType, connectionString);
            }
            catch (Exception e)
            {
                Console.WriteLine("Critical Error! Exception Message = " + e.Message + "; Inner Message = " + e.InnerException?.Message);
            }
        }

        /// <summary>
        /// Start the tracking process for the configurations with parallelized ForEach
        /// </summary>
        /// <param name="configs"></param>
        /// <param name="connectionType"></param>
        /// <param name="connectionString"></param>
        public static void StartTracker(List<TrackingConfiguration> configs, RepositoryType connectionType, string connectionString)
        {
            //var c = configs[0];
            Parallel.ForEach(configs, c =>
            {
                try
                {
                    Console.WriteLine($"{Program.DatePattern()} - Start tracking process for {c.WebsiteConfiguration.Website}({c.WebsiteConfiguration.WebsiteUrl})");

                    ITrackingRepository repo = RepositoryHandler.GetTrackingRepository(connectionType, connectionString);
                    Console.WriteLine($"{Program.DatePattern()} - {c.WebsiteConfiguration.Website} - Database successfully initialized");

                    IDataProvider provider = DataProviderHandler.GetProvider(c.WebsiteConfiguration.DataProviderType);

                    Tracker tracker = new Tracker(repo, provider, c);
                    tracker.StartTracking();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Tracking process for website {c.WebsiteConfiguration.Website} cancelled - Exception Message = " + e.Message + "; Inner Message = " + e.InnerException?.Message);
                }
            });
        }

        public static string DatePattern()
        {
            return DateTime.Now.ToLongTimeString();
        }
    }
}
