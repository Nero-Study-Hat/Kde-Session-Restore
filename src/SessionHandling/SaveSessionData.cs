using System;
using System.Text;
using Newtonsoft.Json;
using KDESessionManager.Objects;
using KDESessionManager.Objects.Configs;

namespace KDESessionManager.SessionHandling
{
    public class SaveSessionData
    {
        public WindowFilter _WindowFilter;
        public string _DynamicOutputPath = "";

        public async Task Process()
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            string[] delimSB = { Environment.NewLine, "\n" };

            Session session = await GetSession(cmdOutputSB, delimSB, _WindowFilter);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);
            await File.WriteAllTextAsync(_DynamicOutputPath, sessionJson);


            Console.WriteLine("Done");
        }

        private async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB, WindowFilter windowFilter)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            int desktopsAmount = await Session.GetNumberOfDesktops(cmdOutputSB);
            List<Window> windows = await Session.GetWindows(cmdOutputSB, delimSB, windowFilter);
            Session session = new Session(activities, windows, desktopsAmount);
            return session;
        }
    }
}