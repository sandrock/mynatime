
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mynatime;
using System;

var configuration = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", false)
   .Build();

var services = new ServiceCollection();
services.AddTransient<App>();
services.AddOptions();
services.Configure<AppSettings>(configuration.GetSection("App"));
services.AddSingleton(new LoggerFactory());
services.AddLogging(builder =>
{
    builder
       .AddDebug();
});

var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetService<App>();
app.Run(args).GetAwaiter().GetResult();
return app.ExitCode;
