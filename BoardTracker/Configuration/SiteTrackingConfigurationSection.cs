using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using CsQuery.ExtensionMethods.Internal;
using DevTracker.BusinessLogic;
using DevTracker.BusinessLogic.DataProvider;
using DevTracker.BusinessLogic.Selector;
using DevTracker.Configuration.Model;
using DevTracker.Model;

namespace DevTracker.Configuration
{

    public class SiteTrackingConfigurationSection : IConfigurationSectionHandler
    {
        /// <summary>
        /// Parse the xml format of the tracking configurations to actual objects
        /// We could use deserialization I guess, I have to test this though
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            Type configType = typeof (TrackingConfiguration);

            List<TrackingConfiguration> configs = new List<TrackingConfiguration>();

            Console.WriteLine("select tracking configuration nodes");
            XmlNodeList trackingProfileConfigurations = section.SelectNodes("trackingConfiguration");
            
            Console.WriteLine("iterate through configs");
            foreach (XmlNode configNode in trackingProfileConfigurations)
            {
                TrackingConfiguration config = new TrackingConfiguration();
                configs.Add(config);

                Console.WriteLine("read name-value properties");
                Dictionary<string, string> attr = new Dictionary<string, string>();
                attr.AddRange(configNode.SelectNodes("add")
                    .Cast<XmlNode>()
                    .Select(
                        x =>
                            new KeyValuePair<string, string>(x.Attributes["key"].InnerText.ToLower(),
                                x.Attributes["value"].InnerText))
                    .ToArray());

                Console.WriteLine("create new website config");
                var webConfig = new WebsiteConfiguration();
                config.WebsiteConfiguration = webConfig;

                Console.WriteLine("set the website that we want to track");
                config.WebsiteConfiguration.Website = attr.FirstOrDefault(x => x.Key.Equals(nameof(webConfig.Website), StringComparison.CurrentCultureIgnoreCase)).Value;

                Console.WriteLine("set website config");
                webConfig.WebsiteUrl = attr.FirstOrDefault(x => x.Key.Equals(nameof(webConfig.WebsiteUrl), StringComparison.CurrentCultureIgnoreCase)).Value;
                webConfig.PostHistoryTemplate = attr.FirstOrDefault(x => x.Key.Equals(nameof(webConfig.PostHistoryTemplate), StringComparison.CurrentCultureIgnoreCase)).Value;

                //set whether the post content should be stored
                if (attr.Any(x => x.Key.Equals(nameof(webConfig.SavePostContent), StringComparison.CurrentCultureIgnoreCase)))
                {
                    webConfig.SavePostContent = bool.Parse(attr.FirstOrDefault(x => x.Key.Equals(nameof(webConfig.SavePostContent), StringComparison.CurrentCultureIgnoreCase)).Value);
                }

                //set the amount of days the tracker should look for updates
                if (attr.Any(x => x.Key.Equals(nameof(webConfig.CheckLastXDaysForUpdates), StringComparison.CurrentCultureIgnoreCase)))
                {
                    webConfig.CheckLastXDaysForUpdates = Int32.Parse(attr.FirstOrDefault(x => x.Key.Equals(nameof(webConfig.CheckLastXDaysForUpdates), StringComparison.CurrentCultureIgnoreCase)).Value);
                }

                Console.WriteLine("set pagination type");
                string paginationTypeConfig = attr.FirstOrDefault(x => x.Key == nameof(webConfig.PaginationType).ToLower()).Value;
                webConfig.PaginationType = (PaginationType)Enum.Parse(typeof(PaginationType), paginationTypeConfig, true);

                Console.WriteLine("set tracker type");
                string trackerType = attr.FirstOrDefault(x => x.Key == nameof(webConfig.DataProviderType).ToLower()).Value ?? "";
                DataProviderType type = DataProviderType.Universal;
                Enum.TryParse(trackerType, true, out type);
                webConfig.DataProviderType = type;

                Console.WriteLine("create new request config");
                var requestConfig = new RequestRateConfiguration();
                config.WebsiteConfiguration.RequestRateConfiguration = requestConfig;

                Console.WriteLine("set request settings");
                requestConfig.RequestRateInMinutes = Int32.Parse(attr.FirstOrDefault(x => x.Key == nameof(requestConfig.RequestRateInMinutes).ToLower()).Value);
                requestConfig.RequestsTillSleep = Int32.Parse(attr.FirstOrDefault(x => x.Key == nameof(requestConfig.RequestsTillSleep).ToLower()).Value);
                requestConfig.RequestTimeoutInMilliseconds = Int32.Parse(attr.FirstOrDefault(x => x.Key == nameof(requestConfig.RequestTimeoutInMilliseconds).ToLower()).Value);
                requestConfig.RequestDelayInMilliseconds = Int32.Parse(attr.FirstOrDefault(x => x.Key == nameof(requestConfig.RequestDelayInMilliseconds).ToLower()).Value);

                Console.WriteLine("set content selectors");
                XmlNode selectorRoot = configNode.SelectSingleNode("contentSelectors");
                XmlNodeList selectorNodes = selectorRoot.SelectNodes("selector");

                List<ContentSelector> selectors = new List<ContentSelector>();
                webConfig.ContentSelectors = selectors;

                foreach (XmlNode selectorNode in selectorNodes)
                {
                    ContentSelector item = new ContentSelector();
                    item.DataType = (ContentSelectorDataType)Enum.Parse(typeof(ContentSelectorDataType), selectorNode.Attributes["dataType"].InnerText);
                    item.ContentSelectorTarget = (ContentSelectorTarget)Enum.Parse(typeof(ContentSelectorTarget), selectorNode.Attributes["target"].InnerText);
                    item.JqSelector = selectorNode.Attributes["jqSelector"].InnerText;
                    item.DateTimeFormat = selectorNode.Attributes["dateTimeFormat"]?.InnerText;

                    Console.WriteLine("test whether a regex pattern which filters the data is specified");
                    if (selectorNode.Attributes["regexPattern"] == null)
                    {
                        item.UseRegexFilter = false;
                    }
                    else
                    {
                        Console.WriteLine("if a pattern exists, set 'UseRegexFilter' to true");
                        item.RegexPattern = selectorNode.Attributes["regexPattern"].InnerText;
                        item.UseRegexFilter = true;

                        Console.WriteLine("set replace string, use $1 if it does not exist");
                        if (selectorNode.Attributes["regexReplace"] != null)
                        {
                            item.RegexReplace = selectorNode.Attributes["regexReplace"].InnerText;
                        }
                        else
                        {
                            item.RegexReplace = "$1";
                        }
                    }

                    if (item.DataType == ContentSelectorDataType.Attribute)
                    {
                        if (selectorNode.Attributes["attributeName"] == null)
                        {
                            Console.WriteLine("You need to set an attribute name for the selector data type 'attribute'");
                        }
                        else
                        {
                            item.AttributeName = selectorNode.Attributes["attributeName"].InnerText;
                        }
                    }

                    selectors.Add(item);
                }

                Console.WriteLine("set tracked profiles");
                config.TrackedProfiles = new List<Profile>();
                var trackedProfilesRoot = configNode.SelectSingleNode("trackedProfiles");

                config.TrackedProfiles.AddRange(trackedProfilesRoot.SelectNodes("profile")
                    .Cast<XmlNode>()
                    .Select(
                        x =>
                            new Profile()
                            {
                                Name = x.Attributes["name"].InnerText,
                                TemplateKey = x.Attributes["templateKey"].InnerText
                            }
                    )
                    .ToArray());
            }

            return configs;
        }
    }
}
