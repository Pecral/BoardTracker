using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTracker.BusinessLogic.Selector
{
    /// <summary>
    /// Save which element of a post the selector is targeting
    /// </summary>
    public enum ContentSelectorTarget
    {
        PostElementThreadTitle,
        PostElementPostingDateTime,
        PostElementContent,
        PostElementForum,
        PostElementForumLink,
        PostElementPostLink,

        PaginationElement,
        PostList
    }
}
