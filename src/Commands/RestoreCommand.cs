using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;

namespace KDESessionManager.Commands
{
    public class RestoreCommand : Command<RestoreCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-f|--file")]
            [Description("Use to pass a specific session file (path) to restore.")]
            // [DefaultValue("")]
            public string File { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            return 0;
        }
    }
}