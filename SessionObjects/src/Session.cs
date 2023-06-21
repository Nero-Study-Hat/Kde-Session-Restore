using System;
using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace SessionObjects
{
    public class Session
    {
        public Dictionary<string, string> Activities { get; set; }
        public int DesktopsAmount { get; set; }
        public Display Display { get; set; }

        public Window[] Windows { get; set; }

        public Session(Dictionary<string, string> activities, Window[] windows, Display display, int desktopsAmount)
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
            string terminalWindowId = await Window.GetActiveTerminalWindowId(cmdOutputSB);
            windowIds.Remove(terminalWindowId);
            windowIds.RemoveAt(windowIds.Count - 1);
            return windowIds.ToArray();
        }

        public static async Task<Window[]> GetWindows(StringBuilder cmdOutputSB, string[] delimSB, string[]? windowIds = null)
        {
            bool debug = false; //FIXME Breakage here.
            windowIds ??= await Session.GetWindowIds(cmdOutputSB, delimSB);
            Window[] windows = new Window[windowIds.Length];
            for (int index = 0; index < windowIds.Length; index++)
            {
                string name = await Window.GetName(windowIds[index], cmdOutputSB);
                int[] asbWinPos = await Window.GetAbsolutePosition(windowIds[index], cmdOutputSB, delimSB);
                string activityId = await Window.GetActivityId(windowIds[index], cmdOutputSB);
                int desktopNum = await Window.GetDesktopNum(windowIds[index], cmdOutputSB);
                string appName = await Window.GetApplicationName(windowIds[index], cmdOutputSB);
                string[] urls = await Window.GetUrls(windowIds[index], cmdOutputSB, delimSB);

                if (index == 13)
                {
                    debug = true;
                }

                windows[index] = new Window(name, windowIds[index], activityId, asbWinPos, desktopNum, appName, urls);
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
            string[] activityIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            cmdOutputSB.Clear();
            Dictionary<string, string> activities = new Dictionary<string, string>();
            for (var i = 0; i < activityIds.Length; i++)
            {
                await Cli.Wrap("qdbus")
                .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", "ActivityName", activityIds[i] })
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(cmdOutputSB))
                .ExecuteBufferedAsync();
                activities.Add(cmdOutputSB.ToString(), activityIds[i]);
                cmdOutputSB.Clear();
            }
            return activities; // FIXME Activity name keys have \n after them.
        }

        public static async Task<int> GetDesktopsAmount(StringBuilder cmdOutputSB)
        {
            cmdOutputSB.Clear();
            Command getNumDesktopsCmd = Cli.Wrap("xprop")
            .WithArguments(new[] { "--root", "-notype", "_NET_NUMBER_OF_DESKTOPS" });
            await (getNumDesktopsCmd | cmdOutputSB).ExecuteBufferedAsync();
            int desktopNum = Int32.Parse(cmdOutputSB.ToString()); //TODO awk cmd to get only number
            cmdOutputSB.Clear();
            return desktopNum;
        }
    }
}