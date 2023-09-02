using CliWrap;
using CliWrap.Buffered;
using System.Text;
using KDESessionManager.Objects.Configs;
using KDESessionManager.Utilities;

namespace KDESessionManager.Objects
{
    public class Session
    {
        public Dictionary<string, string> Activities { get; set; }
        public int DesktopsAmount { get; set; }
        public List<Window> Windows { get; set; }
        public List<Screen> Screens { get; set; }

        public Session(Dictionary<string, string> activities, List<Window> windows, int desktopsAmount, List<Screen> screens)
        {
            Activities = activities;
            DesktopsAmount = desktopsAmount;
            Windows = windows;
            Screens = screens;
        }

        public enum Shortcuts
        {
            Screen_0_Move_to,
            Screen_Next_Move_To,
            Fullscreen_Window,
            Close_Tab,
        }

        public static async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB, WindowFilter windowFilter)
        {
            Dictionary<string, string> activities = await GetActivities(cmdOutputSB, delimSB);
            int desktopsAmount = await GetNumberOfDesktops(cmdOutputSB);
            List<Screen> screens = await GetScreens(cmdOutputSB);
            List<Window> windows = await GetWindows(cmdOutputSB, delimSB, windowFilter, screens);
            Session session = new Session(activities, windows, desktopsAmount, screens);
            return session;
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

        public static async Task<List<Window>> GetWindows(StringBuilder cmdOutputSB, string[] delimSB, WindowFilter windowFilter, List<Screen> screens, string[]? windowIds = null)
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
                
                int asbWinPos = await Window.GetScreenNumber(windowIds[index], screens);

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
            await (BashUtils.QdbusAvtivityCmd("ListActivities") | cmdOutputSB).ExecuteBufferedAsync();
            string[] activityIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None)[0..^1];
            cmdOutputSB.Clear();
            Dictionary<string, string> activities = new Dictionary<string, string>();
            for (var i = 0; i < activityIds.Length; i++)
            {
                await (BashUtils.QdbusAvtivityCmd("ActivityName", activityIds[i]) | cmdOutputSB).ExecuteBufferedAsync();
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

        public static async Task<List<Screen>> GetScreens(StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            string burnerActivityName = "Temp Burner Activity";
            await BashUtils.QdbusAvtivityCmd("AddActivity", burnerActivityName).ExecuteAsync();
            await Task.Delay(2000);
            //TODO: Switch to burner activity and off at cleanup.
            string[] delimSB = { Environment.NewLine, "\n" };
            var activities = await GetActivities(cmdOutputSB, delimSB);
            await BashUtils.QdbusAvtivityCmd("SetCurrentActivity", activities[burnerActivityName]).ExecuteAsync();
            await Task.Delay(2000);
            // Instance a brave browser window.
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "brave";  // FIXME: Support other chromium browsers.
            process.StartInfo.Arguments = "https://google.com" + " --new-window"; 
            process.Start();
            await Task.Delay(2000);
            // Setup window and data.
            Command getWinId = Cli.Wrap("xdotool").WithArguments(new[] { "getwindowfocus" });
            await (getWinId | cmdOutputSB).ExecuteBufferedAsync();
            string winId = cmdOutputSB.ToString();
            cmdOutputSB.Clear();
            await SessionManagerExtensions.RunGivenShortcut(Shortcuts.Screen_0_Move_to);
            await Task.Delay(2000);
            await SessionManagerExtensions.RunGivenShortcut(Shortcuts.Fullscreen_Window);
            await Task.Delay(2000);
            List<Screen> screens = new List<Screen>();
            Screen firstScreen = await Screen.GetScreen(winId);
            Screen newScreen = null!;
            // Setup screens loop and run it to get dimensions for all the users screens.
            //FIXME: Can't enter currently.
            while (firstScreen != newScreen)
            {
                if (newScreen != null) { screens.Add(newScreen); }
                await SessionManagerExtensions.RunGivenShortcut(Shortcuts.Screen_Next_Move_To);
                await Task.Delay(2000);
                newScreen = await Screen.GetScreen(winId);
            }
            // Clean up burner window and activity.
            await SessionManagerExtensions.RunGivenShortcut(Shortcuts.Close_Tab);
            await Task.Delay(2000);
            await BashUtils.QdbusAvtivityCmd("RemoveActivity", burnerActivityName).ExecuteAsync();
            await Task.Delay(2000);
            return screens;
        }
    }
}