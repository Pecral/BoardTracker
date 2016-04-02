namespace BoardTracker.BusinessLogic.Selector
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
