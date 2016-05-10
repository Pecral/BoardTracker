using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardTracker.Model;
using MySql.Data.MySqlClient;

namespace BoardTracker.Repository.MySql
{
    public class TrackingMySQL : ITrackingRepository
    {
        public TrackingMySQL(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private readonly string connectionString;

        #region Connection Stuff


        #endregion Connection Stuff

        /// <summary>
        /// Add posts of a specific author to the database
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="profile"></param>
        public void AddPosts(IEnumerable<Post> posts)
        {
            List<Post> smallBulk = new List<Post>();

            int number = 0;

            foreach (Post post in posts)
            {
                number++;
                smallBulk.Add(post);
                if (number % 5 == 0)
                {
                    AddPostsInSmallBulk(smallBulk);
                    smallBulk.Clear();
                }
            }
        }

        private void AddPostsInSmallBulk(IEnumerable<Post> posts)
        {
            StringBuilder command = new StringBuilder("INSERT INTO Post (ProfileId, PostingDateTime, Content, ThreadTitle, PostLink, Forum, ForumLink) VALUES");
            List<string> rows = new List<string>();
            string row = null;

            foreach (var post in posts)
            {
                string content = post.Content != null ? $"'{MySqlHelper.EscapeString(post.Content)}'" : "null";
                string threadTitle = post.ThreadTitle != null ? $"'{MySqlHelper.EscapeString(post.ThreadTitle)}'" : "null";
                string postLink = post.PostLink != null ? $"'{MySqlHelper.EscapeString(post.PostLink)}'" : "null";
                string forum = post.Forum != null ? $"'{MySqlHelper.EscapeString(post.Forum)}'" : "null";
                string forumLink = post.ForumLink != null ? $"'{MySqlHelper.EscapeString(post.ForumLink)}'" : "null";

                row = "(" +
                    $"{post.ProfileId}, " +
                    $"'{post.PostingDateTime.ToString("o")}', " +
                    $"{content}, " +
                    $"{threadTitle}, " +
                    $"{postLink}, " +
                    $"{forum}, " +
                    $"{forumLink}" +
                    ")";

                rows.Add(row);
            }

            command.Append(string.Join(",", rows));
            command.Append(";");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// Insert a post if it does not already exist, update it if it does
        /// </summary>
        /// <param name="post"></param>
        public void UpsertPost(Post post)
        {
            string eContent = post.Content != null ? $"'{MySqlHelper.EscapeString(post.Content)}'" : "null";
            string eThreadTitle = $"'{MySqlHelper.EscapeString(post.ThreadTitle)}'";
            string ePostLink = $"'{MySqlHelper.EscapeString(post.PostLink)}'";
            string eForum = $"'{MySqlHelper.EscapeString(post.Forum)}'";
            string eForumLink = $"'{MySqlHelper.EscapeString(post.ForumLink)}'";

            StringBuilder command = new StringBuilder("INSERT INTO Post (ProfileId, PostingDateTime, Content, ThreadTitle, PostLink, Forum, ForumLink) VALUES (" +
                                    $"{post.ProfileId}," +
                                    $"{post.ToString()}," +
                                    $"{eContent}," +
                                    $"{eThreadTitle}," +
                                    $"{ePostLink}," +
                                    $"{eForum}," +
                                    $"{eForumLink})" +
                                    $"ON DUPLICATE KEY UPDATE " +
                                        $"Content={eContent}, " +
                                        $"ThreadTitle={eThreadTitle}, " +
                                        $"PostLink={ePostLink}, " +
                                        $"Forum={eForum}, " +
                                        $"ForumLink={eForumLink};");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// Add a new website to the database
        /// </summary>
        /// <param name="website"></param>
        public void UpsertWebsite(Website website)
        {
            string eName = $"'{MySqlHelper.EscapeString(website.Name)}'";
            string eUrl = $"'{MySqlHelper.EscapeString(website.Url)}'";

            StringBuilder command = new StringBuilder("INSERT INTO Website (Name, Url) VALUES (" +
                                    $"{eName}," +
                                    $"{eUrl})" +
                                    $"ON DUPLICATE KEY UPDATE " +
                                        $"Name={eName}, " +
                                        $"Url={eUrl};");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Add a new author's profile of a website to the database
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        public void UpsertProfile(Profile profile)
        {
            string eName = $"'{MySqlHelper.EscapeString(profile.Name)}'";
            string eTemplateKey = $"'{MySqlHelper.EscapeString(profile.TemplateKey)}'";
            string eProfileUrl = profile.ProfileUrl != null ? $"'{MySqlHelper.EscapeString(profile.ProfileUrl)}'" : "null";
            string eAuthorId = profile.AuthorId?.ToString() ?? "null";

            StringBuilder command = new StringBuilder("INSERT INTO Profile (AuthorId, WebsiteId, Name, TemplateKey, ProfileUrl) VALUES (" +
                                    $"{eAuthorId}," +
                                    $"{profile.WebsiteId}," +
                                    $"{eName}," +
                                    $"{eTemplateKey}," +
                                    $"{eProfileUrl})" +
                                    $"ON DUPLICATE KEY UPDATE " +
                                        $"Name={eName}, " +
                                        $"ProfileUrl={eProfileUrl};");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// Add a new author to the database
        /// </summary>
        /// <param name="author"></param>
        public void UpsertAuthor(Author author)
        {
            string eName = $"'{MySqlHelper.EscapeString(author.Name)}'";
            string eLastname = author.Lastname != null ? $"'{MySqlHelper.EscapeString(author.Lastname) ?? ""}'" : "null";
            string eDescription = author.Description != null ? $"'{MySqlHelper.EscapeString(author.Description) ?? ""}'" : "null";

            StringBuilder command = new StringBuilder("INSERT INTO Author (Name, Lastname, Description) VALUES (" +
                                    $"{eName}," +
                                    $"{eLastname})" +
                                    $"{eDescription})" +
                                    $"ON DUPLICATE KEY UPDATE " +
                                        $"Name={eName}, " +
                                        $"Lastname={eLastname}, " +
                                        $"Description={eDescription};");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// Get the date of the last post of a profile
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DateTime? GetDateOfLastPost(Profile profile)
        {
            MySqlDataReader dataReader = null;
            DateTime? postingTime = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string command = $"SELECT MAX(PostingDateTime) " +
                                 $"FROM Post PO " +
                                 $"JOIN Profile PR " +
                                 $"ON PO.ProfileId = PR.ProfileId " +
                                 $"WHERE PR.ProfileId={profile.ProfileId} AND PO.PostingDateTime < '{DateTime.Now.ToString("o")}'";

                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            if (!dataReader.IsDBNull(0))
                            {
                                postingTime = dataReader.GetDateTime(0);
                            }
                        }
                    }
                }
            }

            return postingTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public Author GetAuthor(int authorId)
        {
            MySqlDataReader dataReader = null;
            Author model = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string command = $"SELECT AuthorId, Name, Lastname, Description " +
                                 $"FROM Website" +
                                 $"WHERE AuthorId = {authorId} " +
                                 $"LIMIT 1";

                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            model = new Author();
                            model.AuthorId = dataReader.GetInt32(0);
                            model.Name = dataReader.GetString(1);
                            model.Lastname = dataReader.GetString(2);
                            model.Description = dataReader.GetString(3);
                        }
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Get a saved website by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Website GetWebsite(string name)
        {
            MySqlDataReader dataReader = null;
            Website model = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string command = $"SELECT WebsiteId, Name, Url " +
                                 $"FROM Website " +
                                 $"WHERE Name = '{MySqlHelper.EscapeString(name)}' " +
                                 $"LIMIT 1";

                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            model = new Website();
                            model.WebsiteId = dataReader.GetInt32(0);
                            model.Name = dataReader.GetString(1);
                            model.Url = dataReader.GetString(2);
                        }
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Get the profile with a specific template key
        /// </summary>
        /// <param name="templateKey"></param>
        /// <returns></returns>
        public Profile GetProfile(string templateKey)
        {
            MySqlDataReader dataReader = null;
            Profile model = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string command = $"SELECT ProfileId, AuthorId, WebsiteId, Name, TemplateKey, ProfileUrl " +
                                 $"FROM Profile " +
                                 $"WHERE TemplateKey = '{MySqlHelper.EscapeString(templateKey)}' " +
                                 $"LIMIT 1";

                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(command.ToString(), connection))
                {
                    cmd.CommandType = CommandType.Text;
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            model = new Profile();
                            model.ProfileId = dataReader.GetInt32(0);

                            if (!dataReader.IsDBNull(1))
                            {
                                model.AuthorId = dataReader.GetInt32(1);
                            }

                            model.WebsiteId = dataReader.GetInt32(2);
                            model.Name = dataReader.GetString(3);
                            model.TemplateKey = dataReader.GetString(4);

                            if (!dataReader.IsDBNull(5))
                            {
                                model.ProfileUrl = dataReader.GetString(5);
                            }                 
                        }
                    }
                }
            }

            return model;
        }
    }
}
