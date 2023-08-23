namespace KDESessionManager.Objects.Configs
{
    public class WindowFilter
    {
        public string[]? ApplicationNames { get; set; }
        public string[]? ActivityNames { get; set; }
        public int[]? DesktopNumbers { get; set; }
        public Object? NumberOfRequiredFilters { get; set; }

        public WindowFilter (
            string[]? applicationNames = null,
            string[]? activityNames = null,
            int[]? desktopNumbers = null,
            Object? numberOfRequiredFilters = null)
        {
            ActivityNames = activityNames;
            DesktopNumbers = desktopNumbers;
            ApplicationNames = applicationNames;
            NumberOfRequiredFilters = numberOfRequiredFilters;
        }

        // Return value specifies whether the loop the caller is in continues(true) of not(false).
        public static bool ArrayPropertyCheck<T>(T[]? predicateCollection, T windowValue, List<bool> filterResults, bool severeFilter)
        {
            if (predicateCollection is null || predicateCollection.Length == 0)
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
        { // TODO: Reprompt following invalid error.
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
                Console.WriteLine("Given window filter property (NumberOfRequiredFilters) has an invalid string value.");
                return -1; // Error
            }
            if (windowFilter.NumberOfRequiredFilters is int numberRequired)
            {
                if (numberRequired < 0 || numberRequired > validProperties.Count)
                {
                    Console.WriteLine("Given window filter property (NumberOfRequiredFilters) has an invalid int value.");
                    return -1;
                }
                return numberRequired;
            }
            Console.WriteLine("Given window filter property (NumberOfRequiredFilters) has an invalid value type. Value should be an int or string.");
            return -1; // Error
        }

        public static bool[] GetWindowFilterResults(Window window, WindowFilter windowFilter, List<string> validProperties)
        {
            Dictionary<string, Func<Window, bool>> windowFilters = new Dictionary<string, Func<Window, bool>>()
            {
                {nameof(windowFilter.ApplicationNames), (window) => windowFilter.ApplicationNames!.Contains(window.ApplicationName)},
                {nameof(windowFilter.ActivityNames), (window) => windowFilter.ActivityNames!.Contains(window.Activity[0])},
                {nameof(windowFilter.DesktopNumbers), (window) => windowFilter.DesktopNumbers!.Contains(window.DesktopNum)},
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