using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTracker.BusinessLogic.Selector
{
    public class ContentSelector
    {
        public string JqSelector { get; set; }

        public ContentSelectorTarget ContentSelectorTarget { get; set; }

        /// <summary>
        /// where is the data saved? In the inner html or as a attribute of the dom element?
        /// </summary>
        public ContentSelectorDataType DataType { get; set; }

        /// <summary>
        /// If the selector data type is an attribute, we will save the attribute's name here
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Specify whether the data has to be filtered
        /// </summary>
        public bool UseRegexFilter { get; set; }

        /// <summary>
        /// The regex pattern to filter the data
        /// </summary>
        public string RegexPattern { get; set; }

        /// <summary>
        /// Set the replacement string for the regex pattern e.g. $1 for Group 1
        /// </summary>
        public string RegexReplace { get; set; }

        /// <summary>
        /// A boolean which decides whether special html characters should be decoded ('&amp;' back to '&')
        /// </summary>
        public bool DecodeHtml => DataType != ContentSelectorDataType.DomElement;

        /// <summary>
        /// The date time format for the moment of a post
        /// </summary>
        public string DateTimeFormat { get; set; }
    }
}
