using KDESessionManager.Commands;
using KDESessionManager.Utilities;
using Spectre.Console.Cli;
using CliWrap;
using CliWrap.Buffered;
using Nito.AsyncEx;
using System.Text;

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

        private static async Task HandleDependencies()
        {
            await SetShortcuts();
            var statusPackageDependencies = await CheckPackageDependencies();
            if (statusPackageDependencies)
            {
                Console.WriteLine("Installing linux package dependencies.");
                var installScriptPath = $"{SessionManagerExtensions.TryGetProjectPath().FullName}/LinuxPackageDependenciesInstall.sh";
                var cmdInstallScript = Cli.Wrap("/bin/bash").WithArguments(installScriptPath);
                await (Cli.Wrap("yes") | cmdInstallScript).ExecuteBufferedAsync();
                Console.WriteLine("Finished installing dependencies.");
            }
            if (statusPackageDependencies)
            {
                Console.WriteLine("Linux package dependencies failed to install.");
                //TODO: Error handling.
                // Account for bash script reporting error that no accounted for package manager is present.
                // Report to the user what packages need to be installed and stop the application.
            }
        }

        private static async Task<bool> CheckPackageDependencies()
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

        private static async Task<int> SetShortcuts()
        {
            string shortcutsConfigPath = "~/.config/kglobalshortcutsrc";
            bool shortcutsConfigExists = File.Exists(shortcutsConfigPath);
            if (!shortcutsConfigExists) { return 0; }
            // TODO: Make it possible to choose the shortcuts used by the program in the config, keys speaking.
            // TODO: Prompt user to set up kde shortcuts and try to account for possible file places on the following distros.
            // Arch, Debian, Red Hat

            StringBuilder cmdOutputSB = new StringBuilder();
            CliWrap.Command checkScreenZeroMoveShortcut = Cli.Wrap("kreadconfig5")
            .WithArguments(new[] { "--group", "kwin", "--key", "Window to Screen 2" });
            await (checkScreenZeroMoveShortcut | cmdOutputSB).ExecuteBufferedAsync();
            if (cmdOutputSB.ToString() == "")
            {
                await Cli.Wrap("kwriteconfig5")
                .WithArguments(new[] { "--file", "~/.config/kglobalshortcutsrc", "--group", "kwin", "--key", "Window to Screen 0", "Meta+Ctrl+Alt+3" })
                .ExecuteAsync();
            }
            cmdOutputSB.Clear();
            return 1;
        }
    } 
}