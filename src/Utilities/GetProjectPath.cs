namespace KDESessionManager.Utilities
{
    public static class GetProjectPath
    {
        public static DirectoryInfo TryGetInfo(string? currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("app.anchor").Any())
            {
                directory = directory.Parent;
            }
            return directory!;
        }
    }
}