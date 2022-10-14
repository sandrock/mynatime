
namespace Mynatime.Infrastructure;

using System;

public static class VariousExtensions
{

    /// <summary>
    /// Returns the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Local"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Local"/></returns>
    public static DateTime AsLocal(this DateTime value)
    {
        return new DateTime(value.Ticks, DateTimeKind.Local);
    }

    /// <summary>
    /// Returns the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/></returns>
    public static DateTime AsUtc(this DateTime value)
    {
        return new DateTime(value.Ticks, DateTimeKind.Utc);
    }

    /// <summary>
    /// Returns the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/></returns>
    public static DateTime? AsUtc(this DateTime? value)
    {
        return value != null ? value.Value.AsUtc() : default(DateTime?);
    }

    /// <summary>
    /// Returns the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/></returns>
    public static DateTime AsUnspecified(this DateTime value)
    {
        return new DateTime(value.Ticks, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Returns the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>the date with the same values and a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/></returns>
    public static DateTime? AsUnspecified(this DateTime? value)
    {
        return value != null ? value.Value.AsUnspecified() : default(DateTime?);
    }

    public static DateTime ChangeKind(this DateTime value, DateTimeKind kind)
    {
        return new DateTime(value.Ticks, kind);
    }

    public static DateTime? ChangeKind(this DateTime? value, DateTimeKind kind)
    {
        return value != null ? value.Value.ChangeKind(kind) : default(DateTime?);
    }

}
