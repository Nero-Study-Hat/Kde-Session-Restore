using Newtonsoft.Json;
using KDESessionManager.Objects;
using KDESessionManager.Objects.Configs;
using CliWrap;

namespace KDESessionManager.SessionHandling
{
    public class SaveSessionData
    {
        public async Task Process(WindowFilter windowFilter, DynamicOutputPath dynamicOutputPath)
        {
            string terminalWindowId = await Window.GetActiveWindowId();
            Session session = await Session.GetSession(windowFilter);
            string sessionJson = JsonConvert.SerializeObject(session, Formatting.Indented);
            string outputPath = DynamicOutputPath.GetPath(windowFilter , dynamicOutputPath);
            await File.WriteAllTextAsync(outputPath, sessionJson);
            await Cli.Wrap("xdotool")
            .WithArguments(new[] { "windowactivate", "--sync", terminalWindowId })
            .ExecuteAsync();
            Console.WriteLine("Done");
        }
    }
}