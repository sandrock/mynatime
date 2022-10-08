
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mynatime.CLI;
using Mynatime.Client;
using Mynatime.Infrastructure;
using System;

var configuration = new ConfigurationBuilder()
   ////.SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", false)
   .Build();

var services = new ServiceCollection();
services.AddTransient<ConsoleApp>();
services.AddOptions();
services.Configure<AppSettings>(configuration.GetSection("App"));
services.AddSingleton(new LoggerFactory());
services.AddLogging(builder =>
{
    builder
       .AddDebug();
});
services.AddSingleton<IManatimeWebClient, ManatimeWebClient>();
services.AddSingleton(MynatimeProfileTransactionManager.Default);

var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetService<ConsoleApp>();
app.Run(args).GetAwaiter().GetResult();
return app.ExitCode;
