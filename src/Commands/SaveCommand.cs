using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using KDESessionManager.SessionHandling;
using Nito.AsyncEx;
using KDESessionManager.Objects.Configs;
using Newtonsoft.Json;

namespace KDESessionManager.Commands
{
    public class SaveCommand : Command<SaveCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-l|--live")]
            [Description("Save the session without a loading screen.")]
            [DefaultValue(true)]
            public bool RunLive { get; set; }

            [CommandOption("-o|--output")]
            [Description("Use to present dynamic output path files paths selection from config.")]
            // [DefaultValue(true)]
            public bool OutputNotDefault { get; set; }

            [CommandOption("-f|--filter")]
            [Description("Use to present filter files paths selection from config.")]
            // [DefaultValue(true)]
            public bool FilterNotDefault { get; set; }
        }
 
        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings.RunLive == true)
            {
                AnsiConsole.MarkupLine($"Enjoy the ride!");
            }

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"/home/nero/Workspace/IT_and_Dev/Apps/Current/Kde-Windows-Session/Kde-Session-Restore/data/default_config.jsonc"));
            string strDynamicOutputPath = config.DefaultDynamicOutputPath;
            string strWindowFilterPath = config.DefaultWindowFilterPath;

            var customInputPrompt = "Custom Path Input";


            if (settings.OutputNotDefault == true)
            {
                string[] choices = config.DynamicOutputPathsSelection.Prepend(customInputPrompt).ToArray();
                strDynamicOutputPath = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Choose a DynamicOutput file path[/].")
                        .AddChoices(
                            choices
                        ));
                if (strDynamicOutputPath == customInputPrompt)
                {
                    strDynamicOutputPath = AnsiConsole.Ask<string>("[blue]Input your file path here[/]?");
                }
            }

            if (settings.FilterNotDefault == true)
            {
                string[] choices = config.WindowFiltersPathsSelection.Prepend(customInputPrompt).ToArray();
                strWindowFilterPath = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Choose a WindowFilter file path[/].")
                        .AddChoices(
                            config.WindowFiltersPathsSelection
                        ));
                if (strWindowFilterPath == customInputPrompt)
                {
                    strWindowFilterPath = AnsiConsole.Ask<string>("[blue]Input your file path here[/]?");
                }
            }

            SaveSessionData saveSessionData = new SaveSessionData();
            
            saveSessionData._WindowFilterPath = strWindowFilterPath;
            saveSessionData._DynamicOutputPath = strDynamicOutputPath;
            AsyncContext.Run(saveSessionData.Process);

            return 0;
        }
    }
}