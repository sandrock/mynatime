namespace Mynatime;

using Mynatime.Infrastructure;

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

    TimeZoneInfo TimeZoneLocal { get; }
}
