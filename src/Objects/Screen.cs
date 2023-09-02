using CliWrap;
using CliWrap.Buffered;
using KDESessionManager.Utilities;
using System.Text;

namespace KDESessionManager.Objects
{
    public class Screen
    {
        public int TopBound { get; set; }
        public int BottomBound { get; set; }
        public int LeftBound { get; set; }
        public int RightBound { get; set; }

        public Screen(int topBound, int bottomBound, int leftBound, int rightBound)
        {
            TopBound = topBound;
            BottomBound = bottomBound;
            LeftBound = leftBound;
            RightBound = rightBound;
        }

        public static async Task<Screen> GetScreen(string winId)
        {
            var topBound = await GetTopBound(winId);
            var bottomBound = await GetBottomBound(winId);
            var leftBound = await GetLeftBound(winId);
            var rightBound = await GetRightBound(winId);
            Screen screen = new Screen
            (
                topBound,
                bottomBound,
                leftBound,
                rightBound
            );
            return screen;
        }

        public static async Task<int> GetTopBound(string winId)
        {
            int absY = await FilterWindowData(winId, "Absolute upper-left Y", 4);
            return absY;
        }

        public static async Task<int> GetBottomBound(string winId)
        {
            int absY = await FilterWindowData(winId, "Absolute upper-left Y", 4);
            int height = await FilterWindowData(winId, "Height", 2);
            return absY + height;
        }
        
        public static async Task<int> GetLeftBound(string winId)
        {
            int absX = await FilterWindowData(winId, "Absolute upper-left X", 4);
            return absX;
        }

        public static async Task<int> GetRightBound(string winId)
        {
            int absX = await FilterWindowData(winId, "Absolute upper-left X", 4);
            int width = await FilterWindowData(winId, "Width", 2);
            return absX + width;
        }

        private static async Task<int> FilterWindowData(string winId, string grepPattern, int awkArg)
        {
            var getWinInfoCmd = Cli.Wrap("xwininfo")
            .WithArguments(new[] { "-id", winId });
            var cmdOutput = await BashUtils.FilterCmdGrepAwk(getWinInfoCmd, grepPattern, awkArg);
            int result = Int32.Parse(cmdOutput);
            return result;
        }
    }
}