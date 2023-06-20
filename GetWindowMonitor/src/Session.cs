using System;
using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace SaveSession
{
    public class Session
    {
        public Dictionary<string, string> Activities { get; set; }
        public int DesktopsAmount { get; set; }
        public int[] Resolution { get; set; }

        public Window[] Windows { get; set; }

        public Session(Dictionary<string, string> activities, Window[] windows, int[] resolution, int desktopsAmount)
        {
            Activities = activities;
            DesktopsAmount = desktopsAmount;
            Windows = windows;
            Resolution = resolution;
        }


        public static async Task<string[]> GetWindowIds(StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            Command wmctrlCmd1 = Cli.Wrap("wmctrl")
            .WithArguments("-l");
            Command wmctrlCmd2 = Cli.Wrap("awk")
            .WithArguments("{print $1}");
            await (wmctrlCmd1 | wmctrlCmd2 | cmdOutputSB).ExecuteBufferedAsync();
            string[] windowIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            cmdOutputSB.Clear();
            return windowIds;
        }

        public static async Task<Window[]> GetWindows(StringBuilder cmdOutputSB, string[] delimSB, string[]? windowIds = null)
        {
            windowIds ??= await Session.GetWindowIds(cmdOutputSB, delimSB);
            Window[] windows = new Window[windowIds.Length];
            for (int index = 0; index < windowIds.Length; index++)
            {
                string name = await Window.GetName(windowIds[index], cmdOutputSB, delimSB);
                int[] asbWinPos = await Window.GetAbsolutePosition(windowIds[index], cmdOutputSB, delimSB);
                string activityId = await Window.GetActivityId(windowIds[index], cmdOutputSB, delimSB);
                int desktopNum = await Window.GetDesktopNum(windowIds[index], cmdOutputSB, delimSB);
                string appName = await Window.GetApplicationName(windowIds[index], cmdOutputSB, delimSB);

                windows[index] = new Window(name, windowIds[index], activityId, asbWinPos, desktopNum, appName);
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
            return activities;
        }
    }
}