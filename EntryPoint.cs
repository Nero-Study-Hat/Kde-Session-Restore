using KDESessionManager.Commands;
using KDESessionManager.Utilities;
using Spectre.Console.Cli;
using CliWrap;
using Nito.AsyncEx;

namespace KDESessionManager
{
    public class EntryPoint
    {
        public static int Main(string[] args)
        {
            AsyncContext.Run(HandleDependencies);

            var app = new CommandApp();
            app.Configure(config =>
            {
                config.ValidateExamples();
 
                config.AddCommand<SaveCommand>("save")
                    .WithDescription("Save windows in KDE session..")
                    .WithExample(new[] { "save", "--live" });
            });
 
            return app.Run(args);
        }

        public static async Task HandleDependencies()
        {
            try
            {
                await Cli.Wrap("wmctrl").WithArguments(new[] { "-v" }).ExecuteAsync();
                await Cli.Wrap("xdotool").WithArguments(new[] { "-v" }).ExecuteAsync();
                await Cli.Wrap("xprop").WithArguments(new[] { "-version" }).ExecuteAsync();
            }
            catch (Exception)
            {
                var installScriptPath = $"{GetProjectPath.TryGetInfo().FullName}/LinuxPackageDependenciesInstall";
                await Cli.Wrap("/bin/bash")
                .WithArguments(new[] { installScriptPath })
                .ExecuteAsync();
            }
        }
    } 
}