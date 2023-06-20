using CliWrap;
using CliWrap.Buffered;
using System.Text;

namespace SaveSession
{
    public class ProcessSessionData
    {
        private async static Task Main()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            Console.WriteLine("Done");
        }

        private static async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            Window[] windows = await Session.GetWindows(cmdOutputSB, delimSB);
            Session session = new Session(activities, windows)
            return session;
        }

        // private static async Task<Window[]>FilterWindows(Window[] windows) {} //TODO Window Filtering

        // public string GetMonitor(int[] positionDat)
        // {
        //     //
        // }
    }
}