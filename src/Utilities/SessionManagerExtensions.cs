using CliWrap;
using KDESessionManager.Objects;

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

        public static async Task RunGivenShortcut(Session.Shortcuts shortcut)
        {
            //TODO: Get shortcuts from the main config.
            //TODO: Add the shortcut for for getting the tabs here and in the entry point for setup.

            switch(shortcut)
            {
                case Session.Shortcuts.Screen_0_Move_to:
                    await Cli.Wrap("xdotool")
                    .WithArguments(new[] { "key", "Meta_L+Control_L+Alt_L+1" })
                    .ExecuteAsync();
                    break;
                case Session.Shortcuts.Screen_Next_Move_To:
                    await Cli.Wrap("xdotool")
                    .WithArguments(new[] { "key", "Meta_L+Control_L+Alt_L+1" })
                    .ExecuteAsync();
                    break;
                case Session.Shortcuts.Fullscreen_Window:
                    await Cli.Wrap("xdotool")
                    .WithArguments(new[] { "key", "F11" })
                    .ExecuteAsync();
                    break;
                case Session.Shortcuts.Close_Tab:
                    await Cli.Wrap("xdotool")
                    .WithArguments(new[] { "key", "Control_L+w" })
                    .ExecuteAsync();
                    break;
            }
        }
    }
}