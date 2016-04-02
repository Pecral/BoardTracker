﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTracker.Extensions;
using Model = DevTracker.Model;
using DataContext = DevTracker.Repository.Sql.SqlLinq;



namespace DevTracker.Repository.Sql
{
    public class TrackingSqlLinq : ITrackingRepository
    {
        public TrackingSqlLinq(string connectionString)
        {
            dataContext = new DataContext.TrackingDatabaseDataContext(connectionString);
        }

        private readonly DataContext.TrackingDatabaseDataContext dataContext;

        #region Add Models

        /// <summary>
        /// Add posts of a specific author to the database
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="profile"></param>
        public void AddPosts(IEnumerable<Model.Post> posts)
        {
            List<DataContext.Post> cPosts = new List<DataContext.Post>();
            foreach (var post in posts)
            {
                DataContext.Post cPost = new DataContext.Post();

                DataContext.Mapping.MapToPostEntity(post, cPost);
                cPosts.Add(cPost);
            }

            dataContext.Posts.InsertAllOnSubmit(cPosts);
            dataContext.SubmitChanges();
        }

        public void AddPost(Model.Post post)
        {
            DataContext.Post cPost = new DataContext.Post();

            DataContext.Mapping.MapToPostEntity(post, cPost);
            dataContext.Posts.InsertOnSubmit(cPost);
            dataContext.SubmitChanges();
        }

        public void AddWebsite(Model.Website website)
        {
            DataContext.Website entity = new DataContext.Website();
            DataContext.Mapping.MapToWebsiteEntity(website, entity);

            dataContext.Websites.InsertOnSubmit(entity);
            dataContext.SubmitChanges();
        }

        public void AddProfile(Model.Profile profile)
        {
            DataContext.Profile entity = new DataContext.Profile();
            DataContext.Mapping.MapToProfileEntity(profile, entity);

            dataContext.Profiles.InsertOnSubmit(entity);
            dataContext.SubmitChanges();
        }

        public void AddAuthor(Model.Author author)
        {
            DataContext.Author cAuthor = new DataContext.Author();
            DataContext.Mapping.MapToAuthorEntity(author, cAuthor);

            dataContext.Authors.InsertOnSubmit(cAuthor);
            dataContext.SubmitChanges();
        }

        #endregion Add Models

        #region Get models

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public Model.Author GetAuthor(int authorId)
        {
            DataContext.Author entity = dataContext.Authors.FirstOrDefault(x => x.AuthorId == authorId);

            if (entity != null)
            {
                Model.Author model = new Model.Author();
                DataContext.Mapping.MapToAuthorModel(entity, model);

                return model;
            }

            return null;
        }

        /// <summary>
        /// Get the profile with a specific template key
        /// </summary>
        /// <param name="templateKey"></param>
        /// <returns></returns>
        public Model.Profile GetProfile(string templateKey)
        {
            DataContext.Profile entity = dataContext.Profiles.FirstOrDefault(x => x.TemplateKey == templateKey);

            if (entity != null)
            {
                Model.Profile model = new Model.Profile();
                DataContext.Mapping.MapToProfileModel(entity, model);

                return model;
            }

            return null;
        }

        /// <summary>
        /// Get a saved website by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Model.Website GetWebsite(string name)
        {
            DataContext.Website entity = dataContext.Websites.FirstOrDefault(x => x.Name == name);

            if (entity != null)
            {
                Model.Website model = new Model.Website();
                DataContext.Mapping.MapToWebsiteModel(entity, model);

                return model;
            }

            return null;
        }

        public Model.Post GetPost(string postLink)
        {
            DataContext.Post entity = dataContext.Posts.FirstOrDefault(x => x.PostLink == postLink);

            if (entity != null)
            {
                Model.Post model = new Model.Post();
                DataContext.Mapping.MapToPostModel(entity, model);

                return model;
            }

            return null;
        }


        #endregion

        #region Upserts

        /// <summary>
        /// Insert a post if it does not already exist, update it if it does
        /// </summary>
        /// <param name="post"></param>
        public void UpsertPost(Model.Post post)
        {
            DataContext.Post entity = dataContext.Posts.FirstOrDefault(x => x.PostId == post.PostId);

            if (entity == null)
            {
                AddPost(post);
            }
            else
            {
                DataContext.Mapping.MapToPostEntity(post, entity);
                dataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// Add a new website to the database
        /// </summary>
        /// <param name="website"></param>
        public void UpsertWebsite(Model.Website website)
        {
            DataContext.Website entity = dataContext.Websites.FirstOrDefault(x => x.Name == website.Name);

            if (entity == null)
            {
                AddWebsite(website);
            }
            else
            {
                DataContext.Mapping.MapToWebsiteEntity(website, entity);
                dataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// Add a new author's profile of a website to the database
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        public void UpsertProfile(Model.Profile profile)
        {
            DataContext.Profile entity = dataContext.Profiles.FirstOrDefault(x => x.Name == profile.Name);

            if (entity == null)
            {
                AddProfile(profile);
            }
            else
            {
                DataContext.Mapping.MapToProfileEntity(profile, entity);
                dataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// Add a new author to the database
        /// </summary>
        /// <param name="author"></param>
        public void UpsertAuthor(Model.Author author)
        {
            DataContext.Author entity = dataContext.Authors.FirstOrDefault(x => x.Name == author.Name);

            if (entity == null)
            {
                AddAuthor(author);
            }
            else
            {
                DataContext.Mapping.MapToAuthorEntity(author, entity);
                dataContext.SubmitChanges();
            }
        }

        #endregion Upserts

        /// <summary>
        /// Get the date of the last post of a profile
        /// </summary>
        /// <param name="website"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DateTime? GetDateOfLastPost(Model.Profile profile)
        {
            var entity = dataContext.Profiles.FirstOrDefault(x => x.ProfileId == profile.ProfileId);

            //return null if entity is null, return postingdatetime of the first post if a post exists
            return entity?.Posts.FirstOrDefault()?.PostingDateTime;
        }
    }
}
