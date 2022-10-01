
namespace MynatimeCLI;

using System;

internal static class InternalExtensions
{
    internal static bool AddIfAbsent<T>(this IList<T> source, T item)
    {
        if (source.Contains(item))
        {
            return false;
        }
        else
        {
            source.Add(item);
            return true;
        }
    }
}
