using System;
using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace GetWindowMonitor
{
    public class DataParse
    {

        private async static Task Main()
        {
            var cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            string[] activityIds = await GetActivityIds(cmdOutputSB, delimSB);
            string[] windowIds = await GetWindowIds(cmdOutputSB, delimSB);
            // string[] windowData = await

            //

            Console.WriteLine("Done");
        }

        public static async Task<string[]> GetWindowIds(StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            var wmctrlCmd1 = Cli.Wrap("wmctrl")
            .WithArguments("-l");
            var wmctrlCmd2 = Cli.Wrap("awk")
            .WithArguments("{print $1}");
            await (wmctrlCmd1 | wmctrlCmd2 | cmdOutputSB).ExecuteBufferedAsync();
            string[] windowIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            cmdOutputSB.Clear();
            return windowIds;
        }

        public enum windowData
        {
            WindowId,
            ActivityId,
            AbsoluteWindowPosition,
            DesktopNum,
            ApplicationName
        }

        public static async Task<Dictionary<windowData, Array>>GetWindowData(string[] windowIds, StringBuilder cmdOutputSB, string[] delimSB)
        {
            foreach (var windowId in windowIds)
            {
                int[] absWinPos = await GetAbsoluteWindowPosition(windowId, cmdOutputSB, delimSB);
            }
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

        public static async Task<string[]> GetActivityIds(StringBuilder cmdOutputSB, string[] delimSB)
        {
            cmdOutputSB.Clear();
            await Cli.Wrap("qdbus")
            .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", "ListActivities" })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(cmdOutputSB))
            .ExecuteBufferedAsync();
            string[] activityIds = cmdOutputSB.ToString().Split(delimSB, StringSplitOptions.None);
            cmdOutputSB.Clear();
            return activityIds;
        }

        // public string GetMonitor(int[] positionDat)
        // {
        //     //
        // }
    }
}