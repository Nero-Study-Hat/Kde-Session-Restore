using System;
using System.Text;
using Newtonsoft.Json;
using KDESessionManager.Objects;
using KDESessionManager.Objects.Configs;

namespace KDESessionManager.SessionHandling
{
    public class SaveSessionData
    {
        public string _DynamicOutputPath = "";
        public string _WindowFilterPath = "";

        public async Task Process()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            // WindowFilter windowFilter = new WindowFilter();
            // string windowFilterJson = JsonConvert.SerializeObject(windowFilter, Formatting.Indented);
            // await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/test_filter.json", windowFilterJson);

            Session session = await GetSession(cmdOutputSB, delimSB);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);
            await File.WriteAllTextAsync(_DynamicOutputPath, sessionJson);
            // await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/session.json", sessionJson);


            Console.WriteLine("Done");
        }

        private async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            int desktopsAmount = await Session.GetNumberOfDesktops(cmdOutputSB);

            string windowFilterText = File.ReadAllText("/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/window_filter.json");
            WindowFilter windowFilter = JsonConvert.DeserializeObject<WindowFilter>(windowFilterText) ?? new WindowFilter();

            List<Window> windows = await Session.GetWindows(cmdOutputSB, delimSB, windowFilter);

            static bool testCondition (Window window) { return window.ApplicationName == "brave-browser"; }
            Func<Window, bool>[] testConditions = { testCondition };

            // List<Window> filteredWindows = FilterWindows(windows, testConditions);
            Session session = new Session(activities, windows, desktopsAmount);
            return session;
        }
    }
}