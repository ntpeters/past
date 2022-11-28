namespace past.Core.Test
{
    public class ContentTypeTests
    {
        [Test]
        public void ContentType_All_ContainsAllTypes()
        {
            foreach (var type in Enum.GetValues<ContentType>())
            {
                Assert.That(ContentType.All.HasFlag(type), Is.True);
            }
        }
    }
}
