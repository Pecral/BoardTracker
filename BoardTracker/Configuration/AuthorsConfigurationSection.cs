using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml;
using BoardTracker.Model;
using CsQuery.ExtensionMethods.Internal;

namespace BoardTracker.Configuration
{
    public class AuthorsConfigurationSection : IConfigurationSectionHandler
    {
        /// <summary>
        /// Parse the xml format of the authors-list to actual models
        /// We could use deserialization I guess, I have to test this though
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<Author> authors = new List<Author>();
            Type authorType = typeof (Author);

            XmlNodeList authorsNodeList = section.SelectNodes("author");

            foreach (XmlNode authorsNode in authorsNodeList)
            {
                //create author instance
                Author author = new Author();

                //read name-value properties
                Dictionary<string, string> nameValueProperties = new Dictionary<string, string>();
                nameValueProperties.AddRange(authorsNode.SelectNodes("add")
                    .Cast<XmlNode>()
                    .Select(
                        x =>
                            new KeyValuePair<string, string>(x.Attributes["key"].InnerText.ToLower(),
                                x.Attributes["value"].InnerText))
                    .ToArray());

                //iterate through name value properties, get the field with the same name and set the value
                foreach (var nameValue in nameValueProperties)
                {
                    FieldInfo field = authorType.GetField(nameValue.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (field != null)
                    {
                        var value = Convert.ChangeType(nameValue.Value, field.FieldType);
                        field.SetValue(author, value);
                    }
                }

                authors.Add(author);
            }

            return authors;
        }
    }
}
