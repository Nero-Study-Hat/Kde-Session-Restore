using KDESessionManager.Commands;
using KDESessionManager.Utilities;
using Spectre.Console.Cli;
using CliWrap;
using CliWrap.Buffered;
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
            if (!CheckPackageDependencies().Result)
            {
                Console.WriteLine("Installing linux package dependencies.");
                var installScriptPath = $"{SessionManagerExtensions.TryGetProjectPath().FullName}/LinuxPackageDependenciesInstall.sh";
                var cmdInstallScript = Cli.Wrap("/bin/bash").WithArguments(installScriptPath);
                await (Cli.Wrap("yes") | cmdInstallScript).ExecuteBufferedAsync();
                Console.WriteLine("Finished installing dependencies.");
            }
            if (!CheckPackageDependencies().Result)
            {
                Console.WriteLine("Linux package dependencies failed to install.");
                //TODO: Error handling.
                // Account for bash script reporting error that no accounted for package manager is present.
                // Report to the user what packages need to be installed and stop the application.
            }
        }

        public static async Task<bool> CheckPackageDependencies()
        {
            try
            {
                await Cli.Wrap("xprop").WithArguments(new[] { "-version" }).ExecuteAsync();
                await Cli.Wrap("xwininfo").WithArguments(new[] { "-version" }).ExecuteAsync();
                await Cli.Wrap("wmctrl").WithArguments(new[] { "-V" }).ExecuteAsync();
                await Cli.Wrap("xdotool").WithArguments(new[] { "-V" }).ExecuteAsync();
                return true;
            }
            catch (Exception) { return false; }
        }
    } 
}