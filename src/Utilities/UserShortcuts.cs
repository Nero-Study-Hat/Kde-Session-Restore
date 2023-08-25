using CliWrap;

namespace KDESessionManager.Utilities
{
    public static class UserShortcuts
    {
        public static async Task SetShortcut(string keys)
        {
            //
        }

        public static async Task RunShortcut(string[] keyPresses)
        {
            // TODO: Ensure xdotool is installed.
            IEnumerable<string> args = keyPresses.Prepend("key");
            await Cli.Wrap("xdotool")
            .WithArguments(args)
            .ExecuteAsync();
        }
    }
}