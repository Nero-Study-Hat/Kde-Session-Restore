using System.Text;
using Newtonsoft.Json;
using KDESessionManager.SessionObjects;

namespace KDESessionManager.SaveSession
{
    public class ProcessSessionData
    {
        private async static Task Main()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            // WindowFilter windowFilter = new WindowFilter();
            // string windowFilterJson = JsonConvert.SerializeObject(windowFilter, Formatting.Indented);
            // await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/test_filter.json", windowFilterJson);

            Session session = await GetSession(cmdOutputSB, delimSB);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);
            await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/session.json", sessionJson);


            Console.WriteLine("Done");
        }

        private static async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            Display display = new Display();
            int desktopsAmount = await Session.GetNumberOfDesktops(cmdOutputSB);

            string windowFilterText = File.ReadAllText("/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/window_filter.json");
            WindowFilter windowFilter = JsonConvert.DeserializeObject<WindowFilter>(windowFilterText) ?? new WindowFilter();

            List<Window> windows = await Session.GetWindows(cmdOutputSB, delimSB, windowFilter);

            Func<Window, bool> testCondition = (window) => window.ApplicationName == "brave-browser";
            Func<Window, bool>[] testConditions = { testCondition };

            // List<Window> filteredWindows = FilterWindows(windows, testConditions);
            Session session = new Session(activities, windows, display, desktopsAmount);
            return session;
        }

        private static void ProcessUserInput()
        {
            //
        }
    }
}