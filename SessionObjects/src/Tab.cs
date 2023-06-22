namespace SessionObjects
{
    public class Tab
    {
        public string Title { get; set; }
        public string Url { get; set; }

        public Tab(string title, string url)
        {
            Title = title;
            Url = url;
        }
    }
}