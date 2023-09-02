using System.Text;
using CliWrap;
using CliWrap.Buffered;
using KDESessionManager.Objects;

namespace KDESessionManager.Utilities
{
    public static class BashUtils
    {
        public static Command SimpleGrepFilterCmd(string pattern)
        {
            Command grepCmd = Cli.Wrap("grep")
            .WithArguments(new[] { pattern });
            return grepCmd;
        }

        public static Command AwkFilterArgCmd(int arg)
        {
            string cmdArgument = "{print $" + arg + "}";
            Command awkFilterCmd = Cli.Wrap("awk")
            .WithArguments(new[] { cmdArgument });
            return awkFilterCmd;
        }

        public static async Task<string> FilterCmdGrepAwk(Command cmd, string grepPattern, int awkArg)
        {
            StringBuilder cmdOutputSB = new StringBuilder();
            var grepCmd = SimpleGrepFilterCmd(grepPattern);
            var awkCmd = AwkFilterArgCmd(awkArg);
            await (cmd | grepCmd | awkCmd | cmdOutputSB).ExecuteBufferedAsync();
            string output = cmdOutputSB.ToString();
            return output;
        }

        public static Command QdbusAvtivityCmd(string activityCmd, string? arg = null)
        {
            if (arg == null)
            {
                var cmd = Cli.Wrap("qdbus")
                .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", activityCmd });
                return cmd;
            }
            else
            {
                var cmd = Cli.Wrap("qdbus")
                .WithArguments(new[] { "org.kde.ActivityManager", "/ActivityManager/Activities", activityCmd, arg });
                return cmd;
            }
        }
    }
}