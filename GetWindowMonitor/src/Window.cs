using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace SaveSession
{
    public class Window
    {
        public string Name { get; set; }
        public string WindowId { get; set; }
        public string ActivityId { get; set; }
        public int[] AbsoluteWindowPosition { get; set; }
        public int DesktopNum { get; set; }
        public string ApplicationName { get; set; }

        public Window(string name, string windowId, string activityId, int[] absoluteWindowPosition, int desktopNum, string applicationName)
        {
            Name = name;
            WindowId = windowId;
            ActivityId = activityId;
            AbsoluteWindowPosition = absoluteWindowPosition;
            DesktopNum = desktopNum;
            ApplicationName = applicationName;
        }


        public static async Task<string> GetName(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command xpropCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "WM_NAME" });
            Command awkCmd = Cli.Wrap("awk")
            .WithArguments("{print $3}");
            await (xpropCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] lines = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            string name = TrimCmdOutputEnds(lines[0], 1, 1);
            cmdOutputSB.Clear();
            return name;
        }

        public static async Task<string> GetActivityId(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command xpropCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_KDE_NET_WM_ACTIVITIES" });
            Command awkCmd = Cli.Wrap("awk")
            .WithArguments("{print $3}");
            await (xpropCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] lines = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            string activityId = TrimCmdOutputEnds(lines[0], 1, 1);
            cmdOutputSB.Clear();
            return activityId;
        }

        public static async Task<int[]> GetAbsolutePosition(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command xwininfoCmd = Cli.Wrap("xwininfo")
            .WithArguments(new[] { "-id", windowId });
            Command grepCmd = Cli.Wrap("grep")
            .WithArguments("Absolute");
            Command awkCmd = Cli.Wrap("awk")
            .WithArguments("{print $4}");
            await (xwininfoCmd | grepCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] lines = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            int[] absolutePosition = new int[2];
            absolutePosition[0] = Int32.Parse(lines[0]);
            absolutePosition[0] = Int32.Parse(lines[1]);
            cmdOutputSB.Clear();
            return absolutePosition;
        }

        public static async Task<int> GetDesktopNum(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command xpropCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "_NET_WM_DESKTOP" });
            Command awkCmd = Cli.Wrap("awk")
            .WithArguments("{print $3}");
            await (xpropCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] lines = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            int desktopNum = Int32.Parse(lines[0]);
            cmdOutputSB.Clear();
            return desktopNum;
        }

        public static async Task<string> GetApplicationName(string windowId, StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command xpropCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "-id", windowId, "WM_CLASS" });
            Command awkCmd = Cli.Wrap("awk")
            .WithArguments("{print $3}");
            await (xpropCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string[] lines = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            string appName = TrimCmdOutputEnds(lines[0], 1, 2);
            cmdOutputSB.Clear();
            return appName;
        }

        private static string TrimCmdOutputEnds(string str, int startChars, int endChars)
        {
            string trimStrStart = str.Remove(0,startChars);
            string trimStrEnd = str.Remove(trimStrStart.Length - (endChars + 1),trimStrStart.Length - 1);
            return trimStrEnd;
        }
    }
}