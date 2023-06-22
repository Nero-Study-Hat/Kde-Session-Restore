using CliWrap;
using CliWrap.Buffered;
using System.Text;
using System.IO;
using Newtonsoft.Json;
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
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);

            await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/session.json", sessionJson);

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

        // private static async Task<Window[]>FilterWindows(Window[] windows)
        // {
        //     //
        // }

        // public string GetMonitor(int[] positionDat)
        // {
        //     //
        // }
    }
}