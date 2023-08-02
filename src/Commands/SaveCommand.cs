using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace KDESessionManager.Commands
{
    public class HelloCommand : Command<HelloCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-n|--name <NAME>")]
            [Description("The person or thing to greet.")]
            [DefaultValue("World")]
            public string Name { get; set; }
        }
 
        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.MarkupLine($"Greetings [bold yellow]{settings.Name}[/]!");

            if (settings.Name == "Bob")
            {
                AnsiConsole.MarkupLine($"Bob, you are bloody amazing.");
            }

            return 0;
        }
    }
}