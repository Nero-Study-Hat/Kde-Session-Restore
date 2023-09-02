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

            Session session = await Session.GetSession(cmdOutputSB, delimSB, _WindowFilter);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);
            await File.WriteAllTextAsync(_DynamicOutputPath, sessionJson);


            Console.WriteLine("Done");
        }
    }
}