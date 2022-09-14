
namespace Mynatime.Infrastructure;

using System;

public static class MynatimeConfiguration
{
    internal static string ConfigDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config/mynatime");

    /// <summary>
    /// Gets the configuration directory, creating it if necessary.
    /// </summary>
    /// <returns></returns>
    public static DirectoryInfo GetConfigDirectory()
    {
        var directory = new DirectoryInfo(ConfigDirectory);
        return directory;
    }

    /// <summary>
    /// Gets the configuration directory, creating it if necessary.
    /// </summary>
    /// <returns></returns>
    public static DirectoryInfo EnsureConfigDirectory()
    {
        var directory = new DirectoryInfo(ConfigDirectory);
        if (!directory.Exists)
        {
            directory.Create();
        }

        return directory;
    }
}
