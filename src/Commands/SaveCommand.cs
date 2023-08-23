using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using KDESessionManager.SessionHandling;
using Nito.AsyncEx;
using KDESessionManager.Objects.Configs;
using Newtonsoft.Json;
using NJsonSchema;

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

            // TODO: Validate default config files and generate if any do not exist.
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"/home/nero/Workspace/IT_and_Dev/Apps/Current/Kde-Windows-Session/Kde-Session-Restore/data/default_config.jsonc"))
                            ?? new Config("session", "_", "somethind", new string[]{""}, "something", new string[]{""});
            string strDynamicOutputPath = config.DefaultDynamicOutputPath + GetJsonExt(config.DefaultDynamicOutputPath);
            string strWindowFilterPath = config.DefaultWindowFilterPath + GetJsonExt(config.DefaultWindowFilterPath);

            var customInputPrompt = "Custom Path Input";

            List<string> invalidJsonFiles = new List<string>(); // TODO: Consolidate validation into outside functions.
            foreach (string filePath in config.WindowFiltersPathsSelection)
            {
                var isValidFile = ValidateJsonFile<WindowFilter>(filePath);
                if (isValidFile == false)
                {
                    invalidJsonFiles.Append(filePath);
                }
            }

            if (invalidJsonFiles.Count > 0)
            {
                if (AnsiConsole.Confirm("Would you like to fix your config entries now?"))
                {
                    // TODO: Call function which lets user generate valid JSON configs.
                }
                else { AnsiConsole.WriteLine("Please fix your config entry later."); }
                // TODO: Make sure the user gets a default config if all given configs are broken.
            }


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

            string filterJsonText = File.ReadAllText(strWindowFilterPath);
            WindowFilter windowFilter = JsonConvert.DeserializeObject<WindowFilter>(filterJsonText);
            string outputJsonText = File.ReadAllText(strWindowFilterPath);
            DynamicOutputPath dynamicOutputPath = JsonConvert.DeserializeObject<DynamicOutputPath>(outputJsonText);
            
            saveSessionData._WindowFilter = windowFilter;
            saveSessionData._DynamicOutputPath = DynamicOutputPath.GetPath(windowFilter, dynamicOutputPath); // TODO: Work out what the appropiate path is and send it.
            AsyncContext.Run(saveSessionData.Process);

            return 0;
        }

        private static bool ValidateJsonFile<TObject>(string filterFilePath)
        {
            bool fileExists = File.Exists(filterFilePath);
            if (fileExists == false)
            {
                AnsiConsole.WriteLine($"[bold red]No file exists at the given path ({filterFilePath}). Please replace path with one for a valid JSON file.");
                return false;
            }

            string fileExt = Path.GetExtension(filterFilePath); // FIXME: This is only a string operation on the given path, not pulling the file properties.
            if (fileExt != "json" || fileExt != "jsonc")
            {
                AnsiConsole.WriteLine($"[bold red]The file given by the path ({filterFilePath}) needs to be a JSON file.");
                return false;
            }

            var schema = JsonSchema.FromType<TObject>();

            string jsonText = File.ReadAllText(filterFilePath);

            try
            {
                var jsonErrors = schema.Validate(jsonText);
                if (jsonErrors.Count > 0)
                {
                    foreach (var error in jsonErrors) Console.WriteLine(error);
                    AnsiConsole.WriteLine($"Please correct these errors or replace the given path ({filterFilePath}) with one for a valid JSON file.");
                    return false;
                }
            }
            catch (JsonException)
            {
                AnsiConsole.WriteLine($"[bold red]JSON provided from path ({filterFilePath}) is invalid. Please replace path with one for a valid JSON file.");
                return false;
            }

            return true;

        }

        private static string GetJsonExt(string path)
        {
            bool jsoncFileExists = File.Exists(path + ".jsonc");
            if (jsoncFileExists) { return ".jsonc"; }
            else
            {
                bool jsonFileExists = File.Exists(path + ".json");
                if (jsonFileExists) { return ".json"; }
            }
            return "DNE";
        }
    }
}