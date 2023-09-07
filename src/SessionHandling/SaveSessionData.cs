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
            Session session = await Session.GetSession(_WindowFilter);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);
            await File.WriteAllTextAsync(_DynamicOutputPath, sessionJson);


            Console.WriteLine("Done");
        }
    }
}