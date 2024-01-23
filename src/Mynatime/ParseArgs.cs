
namespace Mynatime.CLI;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Helps parse CLI arguments in a loop.
/// </summary>
public sealed class ParseArgs
{
    private readonly string[] args;
    private int index;
    private readonly StringComparison defaultStringComparison;

    public ParseArgs(StringComparison defaultStringComparison, string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        this.defaultStringComparison = defaultStringComparison;
        this.args = args.ToArray();
        this.index = -1;
    }

    public ParseArgs(string[] args)
        : this(StringComparison.OrdinalIgnoreCase, args)
    {
    }

    /// <summary>
    /// Gets the current index.
    /// </summary>
    public int Index { get => this.index; }

    /// <summary>
    /// Gets the current argument.
    /// </summary>
    public string Current { get => this.args[this.index]; }

    /// <summary>
    /// Moves to the next argument.
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        this.index++;
        return this.index < this.args.Length;
    }

    /// <summary>
    /// Checks whether a quantity of arguments if available after the current one. 
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool Has(int count)
    {
        return this.args.Length - this.index - count > 0;
    }

    /// <summary>
    /// Checks whether the current argument is one of the specified values. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Is(params string[] value)
    {
        return this.Is(this.defaultStringComparison, value);
    }

    /// <summary>
    /// Checks whether the current argument is one of the specified values. 
    /// </summary>
    /// <param name="stringComparison"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Is(StringComparison stringComparison,params string[] value)
    {
        foreach (var value0 in value)
        {
            if (this.Current.Equals(value0, stringComparison))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the remaining items, without moving.
    /// </summary>
    /// <returns></returns>
    public string[] GetNexts()
    {
        var result = new string[this.args.Length - this.index];
        Array.Copy(this.args, this.index, result, 0, result.Length);
        return result;
    }

    /// <summary>
    /// Gets the remaining items, moving to the end.
    /// </summary>
    /// <returns></returns>
    public string[] Remains()
    {
        this.MoveNext();
        var result = new string[this.args.Length - this.index];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = this.Current;
            this.MoveNext();
        }
        
        return result;
    }
}
