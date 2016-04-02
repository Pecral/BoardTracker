using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Linq;
using DevTracker.Model;

namespace DevTracker.Repository.Xml
{
    /// <summary>
    /// DEPRECATED!!
    /// </summary>
    public class TrackingXml : ITrackingRepository
    {
        public TrackingXml(string filePath)
        {
            this.filePath = filePath;
            LoadDatabase(filePath);
        }

        private XDocument database;
        private XElement websitesRoot;
        private XElement authorsRoot;
        private string filePath;

        /// <summary>
        /// Load the xml file if it exists or create a new one if it doesn't exist.
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadDatabase(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.Write("The xml-database does not exist. Specified path: " + filePath);
                Debug.Write("A new file will be created");

                database = new XDocument(new XDeclaration("1.0", "UTF-8", null), new XElement("Database"));
                database.Root.Add(new XElement("Websites"));
                database.Root.Add(new XElement("Authors"));
                database.Save(filePath);
            }
            else
            {
                database = XDocument.Load(filePath);
            }

            websitesRoot = database.Root.Element("Websites");
            authorsRoot = database.Root.Element("Authors");

        }

        #region Does exist and ensure existing

        /// <summary>
        /// Check whether a profile of a specific website is saved
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool DoesProfileExist(Profile profile)
        {
            var xWebsite = GetWebsiteXElement(profile.WebsiteId);

            //return false if website is null, otherwise search for the author
            return xWebsite?.Elements("Profile").Any(x => x.Attribute("name").Value == profile.Name) ?? false;
        }

        /// <summary>
        /// Checks whether the profile post on a specific website exists
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        public bool DoesPostExist(Website website, Profile profile, Post post)
        {
            var xAuthor = GetProfileXElement(profile);

            return xAuthor?.Elements("Post").Any(x => x.Element(nameof(post.PostLink)).Value == post.PostLink) ?? false;
        }

        /// <summary>
        /// Checks whether a specific website exists in the database
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public bool DoesWebsiteExist(Website website)
        {
            return websitesRoot.Elements("Website").Any(x => x.Attribute("name").Value == website.Name);
        }

        /// <summary>
        /// Checks whether a specific author exists in the database
        /// </summary>
        /// <param name="authorId">The author's identifier</param>
        /// <returns></returns>
        public bool DoesAuthorExist(Author author)
        {
            return authorsRoot.Elements("Author").Any(x => x.Attribute("name").Value == author.Name);
        }

        /// <summary>
        /// Ensure the existance of a profile
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        public void EnsureExistingProfile(Website website, Profile profile)
        {
            EnsureExistingWebsite(website);

            if (!DoesProfileExist(profile))
            {
                AddProfile(profile);
            }
        }

        /// <summary>
        /// Ensure the existance of a website
        /// </summary>
        /// <param name="website"></param>
        public void EnsureExistingWebsite(Website website)
        {
            if (!DoesWebsiteExist(website))
            {
                AddWebsite(website);
            }
        }

        #endregion Does exist and ensure existing

        #region Get XElement

        private XElement GetWebsiteXElement(int websiteId)
        {
            return websitesRoot.Elements("Website").FirstOrDefault(x => x.Attribute("websiteId").Value == websiteId.ToString());
        }

        private XElement GetProfileXElement(Profile profile)
        {
            XElement xWebsite = GetWebsiteXElement(profile.WebsiteId);

            //return null if website is null, otherwise search for the author
            return xWebsite?.Elements("Profile").FirstOrDefault(x => x.Attribute("name").Value == profile.Name);
        }

        #endregion

        #region Add
        /// <summary>
        /// Adds a new author to the repository
        /// </summary>
        /// <param name="author"></param>
        public void AddAuthor(Author author)
        {
            authorsRoot.Add(author.ToXElement<Author>());
        }

        /// <summary>
        /// Add a new website to the database
        /// </summary>
        /// <param name="website"></param>
        public void AddWebsite(Website website)
        {
            websitesRoot.Add(website.ToXElement<Website>());
        }

        /// <summary>
        /// Add a new author to the database
        /// </summary>
        /// <param name="profile"></param>
        public void AddProfile(Profile profile)
        {
            var xWebsite = GetWebsiteXElement(profile.WebsiteId);

            if (xWebsite != null)
            {
                xWebsite.Add(profile.ToXElement<Profile>());
                database.Save(filePath);
            }
        }

        /// <summary>
        /// Add the post of a specific profile to the database
        /// </summary>
        /// <param name="post"></param>
        /// <param name="profile"></param>
        public void AddPost(Website website, Profile profile, Post post)
        {
            //only add the post if it doesn't already exist
            if (!DoesPostExist(website, profile, post))
            {
                EnsureExistingProfile(website, profile);
                var xProfile = GetProfileXElement(profile);
                xProfile.Add(post.ToXElement<Post>());

                database.Save(filePath);
            }
        }

        #endregion Add

        #region Get Model
        public Author GetAuthor(int name)
        {
            throw new NotImplementedException();
        }

        public Website GetWebsite(string name)
        {
            throw new NotImplementedException();
        }

        public Profile GetProfile(string templateKey)
        {
            throw new NotImplementedException();
        }

        #endregion Get Model

        /// <summary>
        /// Get the date of the las post of a author in a website
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DateTime? GetDateOfLastPost(Profile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (DoesProfileExist(profile))
            {
                var xAuthor = GetProfileXElement(profile);
                string dateTimeAttribute = nameof(Post.PostingDateTime);

                var lastPostTime = (from p in xAuthor.Elements("Post")
                                    orderby p.Element(dateTimeAttribute).Value descending
                                    select p.Element(dateTimeAttribute).Value).FirstOrDefault();

                if (lastPostTime != null)
                {
                    return DateTime.Parse(lastPostTime);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public void AddPosts(IEnumerable<Post> posts)
        {
            throw new NotImplementedException();
        }

        public void UpsertPost(Post post)
        {
            throw new NotImplementedException();
        }

        public void UpsertWebsite(Website website)
        {
            throw new NotImplementedException();
        }

        public void UpsertProfile(Profile profile)
        {
            throw new NotImplementedException();
        }

        public void UpsertAuthor(Author author)
        {
            throw new NotImplementedException();
        }
    }
}
