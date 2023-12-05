class EditorBase
{
    protected string GetTitleStringOf(string inputString)
    {
        var titleName = string.IsNullOrEmpty(inputString) ? "NO TITLE" : inputString;
        if (titleName.Length > 30)
        {
            titleName = titleName.Substring(0, 30) + "...";
        }
        return titleName;
    }
}

