using CliWrap;
using CliWrap.Buffered;
using System.Text;
using KDESessionManager.Utilities;

namespace KDESessionManager.Objects
{
    public class Window
    {
        public string Name { get; set; }
        public string ApplicationName { get; set; }
        public string[] Activity { get; set; }
        public int DesktopNum { get; set; }
        public int ScreenNum { get; set; }
        public List<Tab> Tabs { get; set; }
        public string Id { get; set; }

        public Window(string name, string[] activity, int absoluteWindowPosition, int desktopNum, string applicationName, List<Tab> tabs, string id)
        {
            Name = name;
            Activity = activity;
            ScreenNum = absoluteWindowPosition;
            DesktopNum = desktopNum;
            ApplicationName = applicationName;
            Tabs = tabs;
            Id = id;
        }


        public static async Task<string> GetName(string windowId)
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            Command getWindowNameCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "WM_NAME" });
            Command sedTrimOutputCmd = Cli.Wrap("sed")
            .WithArguments(new[] {"s/WM_NAME(UTF8_STRING) = //"});
            await (getWindowNameCmd | sedTrimOutputCmd | cmdOutputSB).ExecuteBufferedAsync();
            string name = cmdOutputSB.ToString()[1..^2];
            cmdOutputSB.Clear();
            return name;
        }

        public static async Task<string[]> GetActivity(string windowId)
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            Command getActivityIdCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_KDE_NET_WM_ACTIVITIES" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getActivityIdCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            string activityId = cmdOutputSB.ToString()[1..^2];
            cmdOutputSB.Clear();
            await (BashUtils.QdbusAvtivityCmd("ActivityName", activityId) | cmdOutputSB).ExecuteBufferedAsync();
            string activityName = cmdOutputSB.ToString()[0..^1];
            cmdOutputSB.Clear();
            string[] activity = {activityName, activityId};
            return activity;
        }

        public static async Task<int> GetScreenNumber(string windowId, List<Screen> screens)
        {
            string terminalWindowId = await GetActiveWindowId();
            Command checkWindowStatusCmd = Cli.Wrap("xprop").WithArguments(new[] { "-id", windowId });
            //FIXME: full screen check out put is wrong
            string screenStatus = await BashUtils.FilterCmdGrepAwk(checkWindowStatusCmd, "_NET_WM_STATE(ATOM)", 3);
            bool isFullScreen = screenStatus.TrimEnd( '\r', '\n' ) == "_NET_WM_STATE_FULLSCREEN";

            // Fullscreen browser so dimensions data recieved from xwininfo is accurate with tiling.
            string appName = await GetApplicationName(windowId);
            if(appName == "brave-browser" && isFullScreen == false) // FIXME: Support other chromium browsers.
            {
                await Cli.Wrap("xdotool")
                .WithArguments(new[] { "windowactivate", "--sync", windowId })
                .ExecuteAsync();
                await SessionManagerExtensions.RunGivenShortcut(Session.Shortcuts.Fullscreen_Window);
                await Task.Delay(200);
            }

            var getWinInfoCmd = Cli.Wrap("xwininfo")
            .WithArguments(new[] { "-id", windowId });
            var cmdOutput = await BashUtils.FilterCmdGrepAwk(getWinInfoCmd, "Absolute upper-left X", 4);
            int absX = Int32.Parse(cmdOutput);
            cmdOutput = await BashUtils.FilterCmdGrepAwk(getWinInfoCmd, "Absolute upper-left Y", 4);
            int absY = Int32.Parse(cmdOutput);

            for(int i = 0; i < screens.Count; i++)
            {
                Screen screen = screens[i];
                if (absX >= screen.LeftBound && absX < screen.RightBound && absY >= screen.TopBound && absY < screen.BottomBound)
                {
                    // Unfullscreen window to clean up.
                    if(appName == "brave-browser" && isFullScreen == false) // FIXME: Support other chromium browsers.
                    {
                        await Cli.Wrap("xdotool")
                        .WithArguments(new[] { "windowactivate", "--sync", windowId })
                        .ExecuteAsync();
                        await SessionManagerExtensions.RunGivenShortcut(Session.Shortcuts.Fullscreen_Window);
                        await Task.Delay(200);
                        await Cli.Wrap("xdotool")
                        .WithArguments(new[] { "windowactivate", "--sync", terminalWindowId })
                        .ExecuteAsync();
                    }
                    return i;
                }
            }
            throw new Exception($"Error: Screen not found for window id: {windowId}");
        }

        public static async Task<int> GetDesktopNumber(string windowId)
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            Command getWindowDesktopCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_NET_WM_DESKTOP" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getWindowDesktopCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            int desktopNum = Int32.Parse(cmdOutputSB.ToString()) + 1;
            cmdOutputSB.Clear();
            return desktopNum;
        }

        public static async Task<string> GetApplicationName(string windowId)
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            Command getAppNameCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "WM_CLASS" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getAppNameCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            string appName = cmdOutputSB.ToString()[1..^3];
            cmdOutputSB.Clear();
            return appName;
        }

        public static async Task<List<Tab>> GetTabs(string windowId)
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string terminalWindowId = await GetActiveWindowId();
            Command switchWindowsCmd = Cli.Wrap("xdotool")
            .WithArguments(new[] { "windowactivate", "--sync", windowId });
            Command urlsToClipboardCmd = Cli.Wrap("xdotool")
            .WithArguments(new[] { "key", "alt+c", "alt+c" }); // TODO Configurable Shortcut
            Command urlsToTextCmd = Cli.Wrap("qdbus")
            .WithArguments(new[] { "org.kde.klipper", "/klipper", "getClipboardContents" });
            await switchWindowsCmd.ExecuteAsync();
            await urlsToClipboardCmd.ExecuteAsync();
            await Task.Delay(500);
            await (urlsToTextCmd | cmdOutputSB).ExecuteBufferedAsync();
            await Cli.Wrap("xdotool")
                    .WithArguments(new[] { "windowactivate", "--sync", terminalWindowId })
                    .ExecuteAsync();
            string tabsData = cmdOutputSB.ToString();
            cmdOutputSB.Clear();
            List<Tab> tabs = ProcessTabsData(tabsData);
            return tabs;
        }

        public static async Task<string> GetActiveWindowId()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            Command getActiveWindowIdCmd = Cli.Wrap("xdotool")
            .WithArguments(new[] { "getactivewindow" });
            await (getActiveWindowIdCmd | cmdOutputSB).ExecuteBufferedAsync();
            string intActiveWindowId = cmdOutputSB.ToString()[0..^1];
            cmdOutputSB.Clear();
            Command convertIntWindowIdToHex = Cli.Wrap("printf")
            .WithArguments(new[] { "0x0%x", intActiveWindowId });
            await (convertIntWindowIdToHex | cmdOutputSB).ExecuteAsync();
            string hexActiveWindowId = cmdOutputSB.ToString();
            cmdOutputSB.Clear();
            return hexActiveWindowId;
        }

        private static List<Tab> ProcessTabsData(string data)
        {
            string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            List<Tab> tabs = new List<Tab>();

            for (int i = 0; i < lines.Length; i += 2)
            {
                string cleanTitle = lines[i].Trim();
                cleanTitle = cleanTitle.Replace("\\", "/");
                cleanTitle = cleanTitle.Replace("\"", "'");
                tabs.Add(new Tab(cleanTitle, lines[i + 1]));
            }

            return tabs;
        }
    }
}