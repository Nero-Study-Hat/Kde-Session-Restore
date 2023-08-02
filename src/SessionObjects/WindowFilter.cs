using Newtonsoft.Json;

namespace KDESessionManager.SessionObjects
{
    public class WindowFilter
    {
        public string[]? ApplicationNames { get; set; }
        public string[]? ActivityNames { get; set; }
        public int[]? DesktopNumbers { get; set; }
        public string[]? Names { get; set; }
        public string[]? TabTitles { get; set; }
        public string[]? TabUrls { get; set; }
        public int[]? TabCount { get; set; } //TODO Switch to Min and Max tab count.
        public Object? NumberOfRequiredFilters { get; set; }

        public WindowFilter (
            string[]? applicationNames = null,
            string[]? activityNames = null,
            int[]? desktopNumbers = null,
            string[]? names = null,
            string[]? tabTitles = null,
            string[]? tabUrls = null,
            int[]? tabCount = null,
            Object? numberOfRequiredFilters = null)
        {
            Names = names;
            ActivityNames = activityNames;
            DesktopNumbers = desktopNumbers;
            ApplicationNames = applicationNames;
            TabTitles = tabTitles;
            TabUrls = tabUrls;
            TabCount = tabCount;
            NumberOfRequiredFilters = numberOfRequiredFilters;
        }

        // Return value specifies whether the loop the caller is in continues(true) of not(false).
        public static bool ArrayPropertyCheck<T>(T[]? predicateCollection, T windowValue, List<bool> filterResults, bool severeFilter)
        {
            if (predicateCollection is null)
            {
                return false;
            }
            bool result = predicateCollection.Contains(windowValue);
            if (severeFilter == true)
            {
                if (result == false)
                {
                    return true;
                }
                filterResults.Add(true);
                return false;
            }
            else
            {
                if (result)
                {
                    filterResults.Add(true);
                    return false;
                }
                return false;
            }
        }

        public static bool TabsPropertyCheck<T>(T[]? predicateCollection, T[] tabCollection, List<bool> filterResults, bool severeFilter, bool? tabsSevereFilter = null)
        {
            if (predicateCollection is null)
            {
                return false;
            }
            var intersectProperties = predicateCollection.Intersect(tabCollection);
            if (severeFilter == true)
            {
                if (intersectProperties.Count() != predicateCollection.Length)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (tabsSevereFilter == true && intersectProperties.Count() == predicateCollection.Length)
                {
                    filterResults.Add(true);
                    return false;
                }
                if ((tabsSevereFilter == false || tabsSevereFilter is null) && intersectProperties.Any())
                {
                    filterResults.Add(true);
                    return false;
                }
                return false;
            }
        }

        public static List<string> GetValidPropertyFilters(WindowFilter windowFilter)
        {
            List<string> validProperties = new List<string>();
            foreach(var property in windowFilter.GetType().GetProperties())
            {
                if (property.GetValue(windowFilter) != null)
                {
                    validProperties.Add(property.Name);
                }
            }
            validProperties.Remove(nameof(NumberOfRequiredFilters));
            return validProperties;
        }

        public static int GetNumberOfRequiredFilters(WindowFilter windowFilter)
        {
            List<string> validProperties = GetValidPropertyFilters(windowFilter);
            if (windowFilter.NumberOfRequiredFilters is string amount)
            {
                if (String.Equals(amount, "all", StringComparison.OrdinalIgnoreCase))
                {
                    return validProperties.Count;
                }
                if (String.Equals(amount, "none", StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }
                return -1; // Error
            }
            if (windowFilter.NumberOfRequiredFilters is int numberRequired)
            {
                if (numberRequired < 0 || numberRequired > validProperties.Count)
                {
                    return 0; //FIXME: Reprompt user for new numbers.
                }
                return numberRequired;
            }
            return -1; // Error
        }

        public static bool[] GetWindowFilterResults(Window window, WindowFilter windowFilter, List<string> validProperties)
        {
            Dictionary<string, Func<Window, bool>> windowFilters = new Dictionary<string, Func<Window, bool>>()
            {
                {nameof(windowFilter.ApplicationNames), (window) => windowFilter.ApplicationNames!.Contains(window.ApplicationName)},
                {nameof(windowFilter.ActivityNames), (window) => windowFilter.ActivityNames!.Contains(window.Activity[0])},
                {nameof(windowFilter.DesktopNumbers), (window) => windowFilter.DesktopNumbers!.Contains(window.DesktopNum)},
                {nameof(windowFilter.Names), (window) => windowFilter.Names!.Contains(window.Name)},
                {nameof(windowFilter.TabTitles), (window) => windowFilter.TabTitles!.Intersect(window.Tabs.Select(tab => tab.Title).ToArray()).Any()},
                {nameof(windowFilter.TabUrls), (window) => windowFilter.TabUrls!.Intersect(window.Tabs.Select(tab => tab.Url).ToArray()).Any()},
                {nameof(windowFilter.TabCount), (window) => windowFilter.TabCount!.Contains(window.Tabs.Length)}
            };

            bool[] filterResults = new bool[validProperties.Count];
            for (var i = 0; i < validProperties.Count; i++)
            {
                filterResults[i] = windowFilters[validProperties[i]](window);
            }
            return filterResults;
        }

        public static List<Window> GetAllWindowsFilterResults(Window[] allWindows, WindowFilter windowFilter)
        {
            List<string> validProperties = GetValidPropertyFilters(windowFilter);
            List<Window> approvedWindows = new List<Window>();
            int numFiltersRequired = GetNumberOfRequiredFilters(windowFilter);
            if (numFiltersRequired == -1) { throw new Exception("Error result from number of filters."); }
            foreach (Window window in allWindows)
            {
                bool[] filterResults = GetWindowFilterResults(window, windowFilter, validProperties);
                bool[] filteredApproved = filterResults.Where(result => result == true).ToArray();
                if (filteredApproved.Length >= numFiltersRequired)
                {
                    approvedWindows.Add(window);
                }
            }
            return approvedWindows;
        }


        // public static void GetAllWindowsFilterResults(Window[] allWindows, WindowFilter windowFilter)
        // {
        //     List<string> validProperties = WindowFilter.GetValidPropertyFilters(windowFilter);

        //     Dictionary<string, Func<Window, bool>> windowFilters = new Dictionary<string, Func<Window, bool>>()
        //     {
        //         {nameof(windowFilter.ApplicationNames), (window) => windowFilter.ApplicationNames!.Contains(window.ApplicationName)},
        //         {nameof(windowFilter.ActivityNames), (window) => windowFilter.ActivityNames!.Contains(window.Activity[0])},
        //         {nameof(windowFilter.DesktopNumbers), (window) => windowFilter.DesktopNumbers!.Contains(window.DesktopNum)},
        //         {nameof(windowFilter.Names), (window) => windowFilter.Names!.Contains(window.Name)},
        //         {nameof(windowFilter.TabTitles), (window) => windowFilter.TabTitles!.Intersect(window.Tabs.Select(tab => tab.Title).ToArray()).Any()},
        //         {nameof(windowFilter.TabUrls), (window) => windowFilter.TabUrls!.Intersect(window.Tabs.Select(tab => tab.Url).ToArray()).Any()},
        //         {nameof(windowFilter.TabCount), (window) => windowFilter.TabCount!.Contains(window.Tabs.Length)}
        //     };

        //     List<Window> approvedWindows = new List<Window>();
        //     foreach (string property in validProperties)
        //     {
        //         approvedWindows.AddRange(Array.FindAll(allWindows, (window) => windowFilters[property](window)));
        //     }
        //     Window[] softFilteredWindows = approvedWindows.DistinctBy(window => window.Id).ToArray(); // Only one filter needs to work.
        //     Window[] strictFilteredWindows = approvedWindows.GroupBy(window => window.Id)
        //                                                     .Where(group => group.Count() > 1) // Control the number of filters required to work.
        //                                                     .Select(group => group.First())   // Use (validProperties.Length) to require all filters.
        //                                                     .ToArray();
        // }
    }
}