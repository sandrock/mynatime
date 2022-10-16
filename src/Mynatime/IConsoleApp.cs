namespace Mynatime.CLI;

using Mynatime.Infrastructure;
using Spectre.Console;

public interface IConsoleApp
{
    /// <summary>
    /// Gets or sets the active profile. 
    /// </summary>
    MynatimeProfile? CurrentProfile { get; set; }

    /// <summary>
    /// Gets or sets the discovered profiles. 
    /// </summary>
    IEnumerable<MynatimeProfile> AvailableProfiles { get; }

    DateTime TimeNowLocal { get; }

    DateTime TimeNowUtc { get; }

    TimeZoneInfo TimeZoneLocal { get; }

    IEnumerable<Command> Commands { get; }

    ////IAnsiConsole Console { get; }

    Task PersistProfile(MynatimeProfile profile);
}
