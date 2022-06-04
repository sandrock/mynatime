
namespace MynatimeGUI
{
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Logging;
    using Avalonia.ReactiveUI;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Mynatime.Infrastructure;
    using MynatimeGUI.ViewModels;
    using System;

    class Program
    {
        private static ILogger log;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            var configuration = ConfigureConfiguration();
            ConfigureLogging(configuration);

            var builder = BuildAvaloniaApp();
            var exitCode = builder.StartWithClassicDesktopLifetime(args);
            return exitCode;
        }

        private static IConfiguration ConfigureConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", false)
               .SetBasePath(Environment.CurrentDirectory)
               .Build();
            return configuration;
        }

        private static void ConfigureLogging(IConfiguration configuration)
        {
            // create the logger thing
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                   .AddConsole()
                   .AddConfiguration(configuration.GetSection("Logging"));
            });

            // test log
            log = loggerFactory.CreateLogger(nameof(Program));
            log.LogWarning("starting. ");
            log.LogInformation("starting. ");
            log.LogDebug("starting. ");
            log.LogTrace("starting. ");
            log.LogInformation("default log level is " + configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default").ToString());
            
            // make it available to all things here
            Log.SetLogger(loggerFactory);
            
            // get Avalonia logs too
            Avalonia.Logging.Logger.Sink = new MyAvaloniaLogSink(loggerFactory, LogEventLevel.Warning);
        }

        /// <summary>
        /// Avalonia configuration, don't remove; also used by visual designer.
        /// </summary>
        /// <returns></returns>
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
               .UsePlatformDetect()
               .LogToTrace(LogEventLevel.Debug, LogArea.Layout)
               .UseReactiveUI();
        }
    }
}
