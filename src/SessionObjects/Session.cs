using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace KDESessionManager.SessionObjects
{
    public class Session
    {
        public Dictionary<string, string> Activities { get; set; }
        public int DesktopsAmount { get; set; }
        public Display Display { get; set; }

        public List<Window> Windows { get; set; }

        public Session(Dictionary<string, string> activities, List<Window> windows, Display display, int desktopsAmount)
        {
            Activities = activities;
            DesktopsAmount = desktopsAmount;
            Windows = windows;
            Display = display;
        }


        public static async Task<string[]> GetWindowIds(StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command getWindowIdsCmd = Cli.Wrap("wmctrl")
            .WithArguments("-l");
            Command grepFilterCmd = Cli.Wrap("grep")
            .WithArguments(new[] { "-v", "--", "-1" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $1}"});
            await (getWindowIdsCmd | grepFilterCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            List<string> windowIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None).ToList();
            cmdOutputSB.Clear();
            windowIds.RemoveAt(windowIds.Count - 1);
            return windowIds.ToArray();
        }

        public static async Task<List<Window>> GetWindows(StringBuilder cmdOutputSB, string[] delimSB, WindowFilter? windowFilter = null, string[]? windowIds = null)
        {
            windowIds ??= await Session.GetWindowIds(cmdOutputSB, delimSB);
            List<Window> windows = new List<Window>();
            if (windowFilter is null) // No filtering while running.
            {
                for (int index = 0; index < windowIds.Length; index++)
                {
                    string name = await Window.GetName(windowIds[index], cmdOutputSB);
                    string appName = await Window.GetApplicationName(windowIds[index], cmdOutputSB);
                    string[] activity = await Window.GetActivity(windowIds[index], cmdOutputSB);
                    int desktopNum = await Window.GetDesktopNumber(windowIds[index], cmdOutputSB); //TODO Test - Incorrect for second vscode window. (Test Project Win)
                    int[] asbWinPos = await Window.GetAbsolutePosition(windowIds[index], cmdOutputSB, delimSB);
                    Tab[] tabs = new Tab[1];
                    if(appName == "brave-browser")
                    {
                        tabs = await Window.GetTabs(windowIds[index], cmdOutputSB, delimSB);
                    }

                    windows.Add(new Window(name, activity, asbWinPos, desktopNum, appName, tabs, windowIds[index]));
                }
                return windows;
            }
            else // With filter check after each property is retrieved.
            {
                List<string> validProperties = WindowFilter.GetValidPropertyFilters(windowFilter);
                int numFiltersRequired = WindowFilter.GetNumberOfRequiredFilters(windowFilter);
                bool severeFilter = validProperties.Count == numFiltersRequired;
                if (numFiltersRequired == - 1) { throw new Exception("Error result from number of filters."); }
                List<bool> filterResults = new List<bool>();
                for (int index = 0; index < windowIds.Length; index++)
                {
                    filterResults.Clear();
                    string appName = await Window.GetApplicationName(windowIds[index], cmdOutputSB);
                    bool appNameCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.ApplicationNames, appName, filterResults, severeFilter);
                    if (appNameCheckResult == true) { continue; }
                    string[] activity = await Window.GetActivity(windowIds[index], cmdOutputSB);
                    bool activityCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.ActivityNames, activity[0], filterResults, severeFilter);
                    if (activityCheckResult == true) { continue; }
                    int desktopNum = await Window.GetDesktopNumber(windowIds[index], cmdOutputSB); //TODO Test - Incorrect for second vscode window. (Test Project Win)
                    bool desktopNumCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.DesktopNumbers, desktopNum, filterResults, severeFilter);
                    if (desktopNumCheckResult == true) { continue; }
                    string name = await Window.GetName(windowIds[index], cmdOutputSB);
                    bool nameCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.Names, name, filterResults, severeFilter);
                    if (nameCheckResult == true) { continue; }
                    Tab[] tabs = new Tab[1];
                    if(appName == "brave-browser")
                    {
                        tabs = await Window.GetTabs(windowIds[index], cmdOutputSB, delimSB);
                    }
                    bool tabTitlesCheckResult = WindowFilter.TabsPropertyCheck(windowFilter.TabTitles, tabs.Select(t => t.Title).ToArray(), filterResults, severeFilter, true);
                    if (tabTitlesCheckResult == true) { continue; }
                    bool tabUrlsCheckResult = WindowFilter.TabsPropertyCheck(windowFilter.TabUrls, tabs.Select(t => t.Url).ToArray(), filterResults, severeFilter, true);
                    if (tabUrlsCheckResult == true) { continue; }
                    // TODO Add tab count filter check(s).
                    int[] asbWinPos = await Window.GetAbsolutePosition(windowIds[index], cmdOutputSB, delimSB);

                    bool[] filteredApproved = filterResults.Where(result => result == true).ToArray();
                    if (severeFilter || filteredApproved.Length >= numFiltersRequired)
                    {
                        windows.Add(new Window(name, activity, asbWinPos, desktopNum, appName, tabs, windowIds[index]));
                    }
                }
                return windows;
            }
        }
        
        public static async Task<Dictionary<string, string>> GetActivities(StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            await Cli.Wrap("qdbus")
            .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", "ListActivities" })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(cmdOutputSB))
            .ExecuteBufferedAsync();
            string[] activityIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None)[0..^1];
            cmdOutputSB.Clear();
            Dictionary<string, string> activities = new Dictionary<string, string>();
            for (var i = 0; i < activityIds.Length; i++)
            {
                await Cli.Wrap("qdbus")
                .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", "ActivityName", activityIds[i] })
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(cmdOutputSB))
                .ExecuteBufferedAsync();
                activities.Add(cmdOutputSB.ToString()[0..^1], activityIds[i]);
                cmdOutputSB.Clear();
            }
            return activities; // FIXME Activity name keys have \n after them.
        }

        public static async Task<int> GetNumberOfDesktops(StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            Command getNumDesktopsCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-root", "-notype", "_NET_NUMBER_OF_DESKTOPS" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getNumDesktopsCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            int desktopNum = Int32.Parse(cmdOutputSB.ToString()); //TODO awk cmd to get only number
            cmdOutputSB.Clear();
            return desktopNum;
        }
    }
}