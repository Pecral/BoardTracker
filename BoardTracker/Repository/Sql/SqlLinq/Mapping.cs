namespace BoardTracker.Repository.Sql.SqlLinq
{
    public class Mapping
    {
        #region Entity to Model
        public static void MapToPostModel(Post entity, Model.Post model)
        {
            model.PostId = entity.PostId;
            model.PostLink = entity.PostLink;
            model.Content = entity.Content;
            model.Forum = entity.Forum;
            model.ForumLink = entity.PostLink;
            model.PostingDateTime = entity.PostingDateTime;
            model.ProfileId = entity.ProfileId;
        }

        public static void MapToWebsiteModel(Website entity, Model.Website model)
        {
            model.WebsiteId = entity.WebsiteId;
            model.Name = entity.Name;
            model.Url = entity.Url;
        }

        public static void MapToAuthorModel(Author entity, Model.Author model)
        {
            model.AuthorId = entity.AuthorId;
            model.Description = entity.Description;
            model.Lastname = entity.Lastname;
            model.Name = entity.Name;
        }

        public static void MapToProfileModel(Profile entity, Model.Profile model)
        {
            model.ProfileId = entity.ProfileId;
            model.WebsiteId = entity.WebsiteId;
            model.AuthorId = entity.AuthorId;
            model.Name = entity.Name;
            model.ProfileUrl = entity.ProfileUrl;
            model.TemplateKey = entity.TemplateKey;
        }
        #endregion Entity to Model

        #region Model to Entity

        public static void MapToPostEntity(Model.Post model, Post entity)
        {
            entity.Content = model.Content;
            entity.Forum = model.Forum;
            entity.ForumLink = model.ForumLink;
            entity.PostingDateTime = model.PostingDateTime;
            entity.PostLink = model.PostLink;
            entity.ProfileId = model.ProfileId;
            entity.ThreadTitle = model.ThreadTitle;
        }

        public static void MapToWebsiteEntity(Model.Website model, Website entity)
        {
            entity.Name = model.Name;
            entity.Url = model.Url;
        }

        public static void MapToAuthorEntity(Model.Author model, Author entity)
        {
            entity.Description = model.Description;
            entity.Lastname = model.Lastname;
            entity.Name = model.Name;
        }

        public static void MapToProfileEntity(Model.Profile model, Profile entity)
        {
            entity.AuthorId = model.AuthorId;
            entity.WebsiteId = model.WebsiteId;
            entity.Name = model.Name;
            entity.ProfileUrl = model.ProfileUrl;
            entity.TemplateKey = model.TemplateKey;
        }

        #endregion Model to Entity


    }
}
