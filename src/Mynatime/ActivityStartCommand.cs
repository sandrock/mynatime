
namespace Mynatime;

using MynatimeClient;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class ActivityStartCommand : Command
{
    public ActivityStartCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.TimeZoneLocal = app.TimeZoneLocal;
    }

    public static string[] Args { get; } = new string[] { "start", "stop", };

    public TimeZoneInfo TimeZoneLocal { get; set; }
    public DateTime? StartTimeLocal { get; set; }
    public DateTime? EndTimeLocal { get; set; }
    public decimal? DurationHours { get; set; }
    public string CategoryArg { get; set; }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ActivityCommand.Args);
    }

    public override bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command)
    {
        throw new NotImplementedException();
    }

    public override Task Run()
    {
        throw new NotImplementedException();
    }
}
