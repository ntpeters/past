namespace past.Core
{
    [Flags]
    public enum ContentType
    {
        Text = 1,
        Image = 2,
        File = 4,
        All = Text | Image | File
    }
}
