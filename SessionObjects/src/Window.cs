using CliWrap;
using CliWrap.Buffered;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SessionObjects
{
    public class Window
    {
        public string Name { get; set; }
        public string ApplicationName { get; set; }
        public string WindowId { get; set; }
        public string ActivityId { get; set; }
        public int DesktopNum { get; set; }
        public int[] AbsoluteWindowPosition { get; set; }
        public Tab[] Tabs { get; set; }

        public Window(string name, string windowId, string activityId, int[] absoluteWindowPosition, int desktopNum, string applicationName, Tab[] tabs)
        {
            Name = name;
            WindowId = windowId;
            ActivityId = activityId;
            AbsoluteWindowPosition = absoluteWindowPosition;
            DesktopNum = desktopNum;
            ApplicationName = applicationName;
            Tabs = tabs;
        }


        public static async Task<string> GetName(string windowId, StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            Command getWindowNameCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "WM_NAME" });
            Command sedTrimOutputCmd = Cli.Wrap("sed")
            .WithArguments(new[] {"s/WM_NAME(UTF8_STRING) = //"});
            await (getWindowNameCmd | sedTrimOutputCmd | cmdOutputSB).ExecuteBufferedAsync();
            string name = cmdOutputSB.ToString()[1..^2];
            cmdOutputSB.Clear();
            return name;
        }

        public static async Task<string> GetActivityId(string windowId, StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            Command getWindowActivityCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_KDE_NET_WM_ACTIVITIES" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getWindowActivityCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            string activityId = cmdOutputSB.ToString()[2..^2];
            cmdOutputSB.Clear();
            return activityId;
        }

        public static async Task<int[]> GetAbsolutePosition(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command getWindowPositionCmd = Cli.Wrap("xwininfo")
            .WithArguments(new[] { "-id", windowId });
            Command grepFilterCmd = Cli.Wrap("grep")
            .WithArguments("Absolute");
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $4}"});
            await (getWindowPositionCmd | grepFilterCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] positionDataText = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            int[] absolutePosition = new int[2];
            absolutePosition[0] = Int32.Parse(positionDataText[0]);
            absolutePosition[0] = Int32.Parse(positionDataText[1]);
            cmdOutputSB.Clear();
            return absolutePosition;
        }

        public static async Task<int> GetDesktopNum(string windowId, StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            Command getWindowDesktopCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_NET_WM_DESKTOP" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getWindowDesktopCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            int desktopNum = Int32.Parse(cmdOutputSB.ToString());
            cmdOutputSB.Clear();
            return desktopNum;
        }

        public static async Task<string> GetApplicationName(string windowId, StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            Command getAppNameCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "WM_CLASS" });
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] {"{print $3}"});
            await (getAppNameCmd | awkFilterCmd | cmdOutputSB).ExecuteBufferedAsync();
            string appName = cmdOutputSB.ToString()[1..^3];
            cmdOutputSB.Clear();
            return appName;
        }

        public static async Task<Tab[]> GetUrls(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            if(windowId == "0x06a0000a")
            {
                var debug = true;
            }
            cmdOutputSB.Clear();
            string terminalWindowId = await GetActiveWindowId(cmdOutputSB);
            Command switchWindowsCmd = Cli.Wrap("xdotool")
            .WithArguments(new[] { "windowactivate", "--sync", windowId });
            Command urlsToClipboardCmd = Cli.Wrap("xdotool")
            .WithArguments(new[] { "key", "alt+1", "alt+1" }); // TODO Configurable Shortcut
            Command urlsToTextCmd = Cli.Wrap("qdbus")
            .WithArguments(new[] { "org.kde.klipper", "/klipper", "getClipboardContents" });
            await switchWindowsCmd.ExecuteAsync();
            await urlsToClipboardCmd.ExecuteAsync();
            await Task.Delay(500);
            await (urlsToTextCmd | cmdOutputSB).ExecuteBufferedAsync();
            await Cli.Wrap("xdotool")
                    .WithArguments(new[] { "windowactivate", "--sync", terminalWindowId })
                    .ExecuteAsync();
            string dirtyTabsJson = cmdOutputSB.ToString();
            string cleanTabsJson = Regex.Replace(dirtyTabsJson, @"\\", "/");
            cmdOutputSB.Clear();
            Tab[] tabs = JsonConvert.DeserializeObject<Tab[]>(cleanTabsJson) ?? new Tab[1];
            return tabs;
        }

        public static async Task<string> GetActiveWindowId(StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
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
    }
}