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
        
        public WindowFilter
        (string[]? name = null, string[]? activityNames = null, int[]? desktopNumbers = null, string[]? applicationNames = null, Tab[]? tabs = null)
        {
            Names = name;
            ActivityNames = activityNames;
            DesktopNumbers = desktopNumbers;
            ApplicationNames = applicationNames;
            Tabs = tabs;
        }

        // bool return is continue status
        // public static bool ArrayPropertyCheck<T>(T[]? predicates, T value, bool severeFilter, bool desirableStatus)
        // {
        //     if(predicates is null)
        //     {
        //         return false;
        //     }
        //     desirableStatus = predicates.Contains(value);
        //     if (severeFilter == true && desirableStatus == false)
        //     {
        //         return true;
        //     }
        //     if (severeFilter == false && desirableStatus == true)
        //     {
        //         return true;
        //     }
        //     return false;
        // }

        public static (bool continueStatus, bool desirableStatus) ApplyValuePropertyFilter<T>(T? predicate, T value, bool severeFilter)
        {
            bool status = false;
            if(predicate is null)
            {
                return (continueStatus: false, desirableStatus: status);
            }
            status = predicate.Equals(value);
            if (severeFilter == true && status == false)
            {
                return (continueStatus: true, desirableStatus: status);
            }
            if (severeFilter == false && status == true)
            {
                return (continueStatus: true, desirableStatus: status);
            }
            return (continueStatus: false, desirableStatus: status);
        }
    }
}