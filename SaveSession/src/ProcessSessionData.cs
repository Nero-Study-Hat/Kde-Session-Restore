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

            WindowFilter windowFilter = new WindowFilter();
            string windowFilterJson = JsonConvert.SerializeObject(windowFilter, Formatting.Indented);
            await File.WriteAllTextAsync($"/home/nero_admin/Workspace/IT/Kde-Session-Restore/data/test_filter.json", windowFilterJson);

            Console.WriteLine("Done");
        }

        private static async Task<Session>GetSession(StringBuilder cmdOutputSB, string[] delimSB)
        {
            Dictionary<string, string> activities = await Session.GetActivities(cmdOutputSB, delimSB);
            Display display = new Display();
            int desktopsAmount = await Session.GetDesktopsAmount(cmdOutputSB);
            Window[] windows = await Session.GetWindows(cmdOutputSB, delimSB);

            Func<Window, bool> testCondition = (window) => window.ApplicationName == "brave-browser";
            Func<Window, bool>[] testConditions = { testCondition };

            Window[] filteredWindows = FilterWindows(windows, testConditions);
            Session session = new Session(activities, windows, display, desktopsAmount);
            return session;
        }

        private static Window[] ConfigFilterWindows(Window[] allWindows, string filterConfigPath)
        {
            string filterConfigText = File.ReadAllText(filterConfigPath);
            WindowFilter windowFilter = JsonConvert.DeserializeObject<WindowFilter>(filterConfigText) ?? new WindowFilter();
            return allWindows;
        }

        private static Window[] FilterWindows(Window[] allWindows, Func<Window,bool>[] filterConditions)
        {
            List<Window> windowsFiltered = new List<Window>();
            foreach(Window window in allWindows)
            {
                bool wanted = true;
                int i = 0;
                while(wanted == true && i < filterConditions.Length)
                {
                    wanted = filterConditions[i](window);
                    i++;
                }
                if(wanted == true)
                {
                    windowsFiltered.Add(window);
                }
            }
            return windowsFiltered.ToArray();
        }

        // public string GetMonitor(int[] positionDat)
        // {
        //     //
        // }
    }
}