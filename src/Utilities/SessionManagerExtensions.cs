using CliWrap;

namespace KDESessionManager.Utilities
{
    public static class SessionManagerExtensions
    {
        public static DirectoryInfo TryGetProjectPath(string? currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            var exit = directory.GetFiles("app.anchor").Any();
            while (directory != null && !exit)
            {
                directory = directory.Parent;
            }
            if(directory == null) { throw new Exception("Missing app.anchor file in project."); }
            return directory;
        }

        public static async Task SetShortcut(string keys)
        {
            //
        }

        public static async Task RunShortcut(string[] keyPresses)
        {
            IEnumerable<string> args = keyPresses.Prepend("key");
            await Cli.Wrap("xdotool")
            .WithArguments(args)
            .ExecuteAsync();
        }
    }
}