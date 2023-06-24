using System.Text;
using Newtonsoft.Json;
using SessionObjects;

namespace SaveSession
{
    public class ProcessSessionData
    {
        private async static Task Main()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            Session session = await GetSession(cmdOutputSB, delimSB);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);

            await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/session.json", sessionJson);

            WindowFilter windowFilter = new WindowFilter();
            string windowFilterJson = JsonConvert.SerializeObject(windowFilter, Formatting.Indented);
            await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/test_filter.json", windowFilterJson);

            Console.WriteLine("Done");
        }

        private static async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            Display display = new Display();
            int desktopsAmount = await Session.GetDesktopsAmount(cmdOutputSB);
            List<Window> windows = await Session.GetWindows(cmdOutputSB, delimSB);

            Func<Window, bool> testCondition = (window) => window.ApplicationName == "brave-browser";
            Func<Window, bool>[] testConditions = { testCondition };

            // List<Window> filteredWindows = FilterWindows(windows, testConditions);
            Session session = new Session(activities, windows, display, desktopsAmount);
            return session;
        }

        // private static List<Window> ConfigFilterWindows(List<Window> allWindows, string filterConfigPath, bool severeFilter)
        // {
        //     List<Window> filteredWindows = new List<Window>();
        //     string filterConfigText = File.ReadAllText(filterConfigPath);
        //     WindowFilter windowFilter = JsonConvert.DeserializeObject<WindowFilter>(filterConfigText) ?? throw new ArgumentNullException("Window filter json failed: ", nameof(windowFilter));
        //     //
            
        //     return allWindows;
        // }

        //TODO Consider brainstrorming on inputing the property to filter, along with arrays and such.
        // Consider choosing filters through -> config file, user input for list of choices, and func param.
        public static void StrictWindowFilter(Window[] allWindows, WindowFilter windowFilter, string[] filterKeys)
        {
            // var validPropertyFilters = windowFilter
            //     .GetType()
            //     .GetProperties()
            //     .Select(p => p.GetValue(windowFilter))
            //     .Where(x => x != null || ( x is string y && y != ""))
            //     .ToList();
            foreach(var property in windowFilter.GetType().GetProperties())
            {
                if (property.GetValue(windowFilter) == null)
                {
                    // Add string to valid array.
                }
            }

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
            Window[] filteredWindows = Array.FindAll(allWindows, (window) => windowFilters[filterKeys[0]](window));
            // Window[] filteredWindows = Array.FindAll(allWindows, (window) => windowFilters[nameof(windowFilter.ApplicationNames)](window));
        }


        private static Window[] FilterWindows(Window[] allWindows, Func<Window,bool>[] filterConditions)
        {
            List<Window> windowsFiltered = new List<Window>();
            foreach(Window window in allWindows)
            {
                bool wanted = true;
                int i = 0;
                while(wanted == true && i < filterConditions.Length)
                {
                    wanted = filterConditions[i](window);
                    i++;
                }
                if(wanted == true)
                {
                    windowsFiltered.Add(window);
                }
            }
            return windowsFiltered.ToArray();
        }

        // public string GetMonitor(int[] positionDat)
        // {
        //     //
        // }
    }
}