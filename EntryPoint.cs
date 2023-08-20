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
 
                config.AddCommand<SaveCommand>("save")
                    .WithDescription("Save windows in KDE session..")
                    .WithExample(new[] { "save", "--live" });
            });
 
            return app.Run(args);
        }
    } 
}