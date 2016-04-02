using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTracker.BusinessLogic.Selector
{
    /// <summary>
    /// Save where the data is saved actually.
    /// The data will be either an attribute of the dom element or the inner html itself.
    /// </summary>
    public enum ContentSelectorDataType
    {
        Html,
        Attribute,
        DomElement
    }
}
