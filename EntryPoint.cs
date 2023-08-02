using KDESessionManager.Commands;
using Spectre.Console.Cli;

namespace KDESessionManager
{
    public class EntryPoint
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.ValidateExamples();
 
                config.AddCommand<HelloCommand>("hello")
                    .WithDescription("Say hello to anyone.")
                    .WithExample(new[] { "hello", "--name", "DarthPedro" });
            });
 
            return app.Run(args);
        }
    } 
}