namespace past.Core
{
    [Flags]
    public enum ContentType
    {
        Text = 1,
        Image = 2,
        All = Text | Image
    }
}
