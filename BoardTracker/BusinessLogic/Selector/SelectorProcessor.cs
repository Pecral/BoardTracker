using System.Text.RegularExpressions;
using System.Web;
using CsQuery;
using BoardTracker.Extensions;

namespace BoardTracker.BusinessLogic.Selector
{

    public class SelectorProcessor
    {
        /// <summary> 
        /// This class will search data on a dom element with a specific selector and filter it with a regex pattern if needed
        /// </summary>
        public static string ProcessContentSelector(ContentSelector selector, CQ dom)
        {
            CQ targetElement = dom[selector.JqSelector];

            string data;

            switch (selector.DataType)
            {
                case ContentSelectorDataType.Attribute:
                    data = targetElement.Attr(selector.AttributeName);
                    break;

                case ContentSelectorDataType.Html:
                    data = targetElement.Html();
                    break;

                case ContentSelectorDataType.Text:
                    data = targetElement.Text();
                    break;

                default:
                    return "";
            }

            //filter the data with a pattern if it's activated
            if (data != null)
            {
                if (selector.UseRegexFilter)
                {
                    data = Regex.Replace(data, selector.RegexPattern, selector.RegexReplace, RegexOptions.Singleline);
                }

                if (selector.DecodeHtml)
                {
                    data = HttpUtility.HtmlDecode(data);
                }
            }

            return data;
        }
    }
}
