using CliWrap;
using CliWrap.Buffered;
using System.Text;
using KDESessionManager.Objects.Configs;

namespace KDESessionManager.Objects
{
    public class Session
    {
        public Dictionary<string, string> Activities { get; set; }
        public int DesktopsAmount { get; set; }
        public List<Window> Windows { get; set; }

        public Session(Dictionary<string, string> activities, List<Window> windows, int desktopsAmount)
        {
            Activities = activities;
            DesktopsAmount = desktopsAmount;
            Windows = windows;
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

        public static async Task<List<Window>> GetWindows(StringBuilder cmdOutputSB, string[] delimSB, WindowFilter windowFilter, string[]? windowIds = null)
        {
            windowIds ??= await Session.GetWindowIds(cmdOutputSB, delimSB);
            List<Window> windows = new List<Window>();
            List<string> validProperties = WindowFilter.GetValidPropertyFilters(windowFilter);
            int numFiltersRequired = WindowFilter.GetNumberOfRequiredFilters(windowFilter);
            bool severeFilter = validProperties.Count == numFiltersRequired; // TODO: Document how this works later, confusing to look at.
            if (numFiltersRequired == - 1) { throw new Exception("Error result from number of filters."); }
            List<bool> filterResults = new List<bool>();
            for (int index = 0; index < windowIds.Length; index++)
            {
                filterResults.Clear();
                string appName = await Window.GetApplicationName(windowIds[index], cmdOutputSB);
                bool appNameCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.ApplicationNames, appName, filterResults, severeFilter);
                if (appNameCheckResult == true) { continue; }

                string[] activity = await Window.GetActivity(windowIds[index], cmdOutputSB);
                bool activityCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.ActivityNames, activity[0], filterResults, severeFilter); // FIXME: The filter has no activity requirement yet this is giving true.
                if (activityCheckResult == true) { continue; }

                int desktopNum = await Window.GetDesktopNumber(windowIds[index], cmdOutputSB);
                bool desktopNumCheckResult = WindowFilter.ArrayPropertyCheck(windowFilter.DesktopNumbers, desktopNum, filterResults, severeFilter);
                if (desktopNumCheckResult == true) { continue; }

                string name = await Window.GetName(windowIds[index], cmdOutputSB);

                List<Tab> tabs = new List<Tab>();
                
                if(appName == "brave-browser") // FIXME: Support other chromium browsers.
                {
                    tabs = await Window.GetTabs(windowIds[index], cmdOutputSB, delimSB);
                }
                
                int[] asbWinPos = await Window.GetAbsolutePosition(windowIds[index], cmdOutputSB, delimSB);

                bool[] filteredApproved = filterResults.Where(result => result == true).ToArray();
                if (severeFilter || filteredApproved.Length >= numFiltersRequired)
                {
                    windows.Add(new Window(name, activity, asbWinPos, desktopNum, appName, tabs, windowIds[index]));
                }
            }
            return windows;
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

        public static async Task GetScreenData(StringBuilder cmdOutputSB)
        {
            string burnerActivityName = "Temp Burner Activity";
            cmdOutputSB.Clear();
            await Cli.Wrap("qdbus")
            .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", "AddActivity", burnerActivityName })
            .ExecuteBufferedAsync();

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "brave";
            process.StartInfo.Arguments = "https://google.com" + " --new-window"; 
            process.Start();

            // TODO: set shortcut if it does not exist
            await Cli.Wrap("xdotool") // shortcut to move window to screen 0
            .WithArguments(new[] { "key", "Meta_L+Control_L+Alt_L+1" })
            .ExecuteAsync();

            
            // TODO: Get screen data bash implementation. 
            // Then run the shortcut to move the window to screen 0.
            // Then get the window geometry data and after that run the shortcut to move to the next screen.
            // Stop when returned window geometry already exists in data.

            await Cli.Wrap("qdbus")
            .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", "RemoveActivity", burnerActivityName })
            .ExecuteBufferedAsync();
        }
    }
}