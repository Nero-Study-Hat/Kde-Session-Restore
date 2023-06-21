using CliWrap;
using CliWrap.Buffered;
using System.Text;
using SessionObjects;

namespace SaveSession
{
    public class ProcessSessionData
    {
        private async static Task Main()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            Session session = await GetSession(cmdOutputSB, delimSB);

            Console.WriteLine("Done");
        }

        private static async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            Window[] windows = await Session.GetWindows(cmdOutputSB, delimSB);
            Display display = new Display();
            int desktopsAmount = await Session.GetDesktopsAmount(cmdOutputSB);
            Session session = new Session(activities, windows, display, desktopsAmount);
            return session;
        }

        // private static async Task<Window[]>FilterWindows(Window[] windows) {} //TODO Window Filtering

        // public string GetMonitor(int[] positionDat)
        // {
        //     //
        // }
    }
}