using Newtonsoft.Json;

namespace SessionObjects
{
    public class WindowFilter
    {
        public string[]? Names { get; set; }
        public string[]? ApplicationNames { get; set; }
        public string[]? ActivityNames { get; set; }
        public int[]? DesktopNumbers { get; set; }
        public Tab[]? Tabs { get; set; }
        
        public WindowFilter(string[]? name = null, string[]? activityNames = null, int[]? desktopNumbers = null, string[]? applicationNames = null, Tab[]? tabs = null)
        {
            Names = name;
            ActivityNames = activityNames;
            DesktopNumbers = desktopNumbers;
            ApplicationNames = applicationNames;
            Tabs = tabs;
        }
    }
}