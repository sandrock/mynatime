
namespace Mynatime.GUI.Things;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Utility
{
    public static void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var fixedUrl = url.Replace("&", "^&");
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("start", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            Process.Start("open", url);
        }
    }
}
