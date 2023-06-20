using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace GetWindowMonitor
{
    public class Window
    {
        public string name { get; set; }
        public string windowId { get; set; }
        public string activityId { get; set; }
        public int[] absoluteWindowPosition { get; set; }
        public int desktopNum { get; set; }
        public string applicationName { get; set; }

        public Window(string Name, string WindowId, string ActivityId, int[] AbsoluteWindowPosition, int DesktopNum, string ApplicationName)
        {
            name = Name;
            windowId = WindowId;
            activityId = ActivityId;
            absoluteWindowPosition = AbsoluteWindowPosition;
            desktopNum = DesktopNum;
            applicationName = ApplicationName;
        }


        public static async Task<string> GetActivity(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            var xpropCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_KDE_NET_WM_ACTIVITIES" });
        }

        public static async Task<int[]> GetAbsoluteWindowPosition(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            var xwininfoCmd = Cli.Wrap("xwininfo")
            .WithArguments(new[] { "-id", windowId });
            var grepCmd = Cli.Wrap("grep")
            .WithArguments("Absolute");
            var awkCmd = Cli.Wrap("awk")
            .WithArguments("{print $4}");
            await (xwininfoCmd | grepCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] lines = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            int[] absoluteWindowPosition = new int[2];
            absoluteWindowPosition[0] = Int32.Parse(lines[0]);
            absoluteWindowPosition[0] = Int32.Parse(lines[1]);
            cmdOutputSB.Clear();
            return absoluteWindowPosition;
        }
    }
}