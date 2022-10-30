
namespace Mynatime.Infrastructure;

using System;

public static class MynatimeProfiles
{
    public static IEnumerable<FileInfo> DiscoverProfileFiles(string directory)
    {
        return DiscoverProfileFiles(new DirectoryInfo(directory));
    }

    public static IEnumerable<FileInfo> DiscoverProfileFiles(DirectoryInfo directory)
    {
        if (directory == null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (directory.Exists)
        {
            foreach (var file in directory.EnumerateFiles("*.json"))
            {
                yield return file;
            }
        }
    }
}
