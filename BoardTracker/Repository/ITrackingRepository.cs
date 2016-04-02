using System;
using System.Collections.Generic;
using BoardTracker.Model;

namespace BoardTracker.Repository
{
    public interface ITrackingRepository
    {
        #region Add models
        /// <summary>
        /// Add posts of a specific author to the database
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="profile"></param>
        void AddPosts(IEnumerable<Post> posts);

        /// <summary>
        /// Insert a post if it does not already exist, update it if it does
        /// </summary>
        /// <param name="post"></param>
        void UpsertPost(Post post);

        /// <summary>
        /// Add a new website to the database
        /// </summary>
        /// <param name="website"></param>
        void UpsertWebsite(Website website);

        /// <summary>
        /// Add a new author's profile of a website to the database
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        void UpsertProfile(Profile profile);

        /// <summary>
        /// Add a new author to the database
        /// </summary>
        /// <param name="author"></param>
        void UpsertAuthor(Author author);

        #endregion Add Models

        /// <summary>
        /// Get the date of the last post of a profile
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        DateTime? GetDateOfLastPost(Profile profile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns></returns>
        Author GetAuthor(int authorId);

        /// <summary>
        /// Get a saved website by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Website GetWebsite(string name);

        /// <summary>
        /// Get the profile with a specific template key
        /// </summary>
        /// <param name="templateKey"></param>
        /// <returns></returns>
        Profile GetProfile(string templateKey);
    }
}
