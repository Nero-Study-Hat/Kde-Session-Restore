using System;
using System.Collections.Generic;
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

            // Dictionary<WindowData, Window> = await 

            Console.WriteLine("Done");
        }


        public static async Task<Window[]>GetWindowData(string[] windowIds, StringBuilder cmdOutputSB, string[] delimSB)
        {
            var windows = new Window[windowIds.Length];
            for (int index = 0; index < windowIds.Length; index++)
            {
                // string activity = await 
                int[] asbWinPos = await GetAbsoluteWindowPosition(windowIds[index], cmdOutputSB, delimSB);

                windows[index] = new Window();
            }
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