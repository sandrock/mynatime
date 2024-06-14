
namespace Mynatime.Client;

using Microsoft.AspNetCore.WebUtilities;
using Mynatime.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

/// <summary>
/// <para>Container for web form values. With utility methods. </para>
/// <para>Stores keys and values to help parse or submit web forms. </para>
/// </summary>
public sealed class WebForm
{
    private readonly Dictionary<string, WebFormValues> keys = new Dictionary<string, WebFormValues>();
    private readonly string method;
    private readonly string action;

    /// <summary>
    /// Creates a new empty web form.
    /// </summary>
    public WebForm()
    {
    }

    /// <summary>
    /// Creates a web form with preset keys. 
    /// </summary>
    /// <param name="keys"></param>
    public WebForm(params string[] keys)
    {
        foreach (var key in keys)
        {
            this.keys.Add(key, new WebFormValues());
        }
    }

    /// <summary>
    /// Creates a web form with preset keys. 
    /// </summary>
    /// <param name="keys"></param>
    public WebForm(string method, string action, string[] keys)
    {
        this.method = method;
        this.action = action;
        foreach (var key in keys)
        {
            this.keys.Add(key, new WebFormValues());
        }
    }

    public string FormMethod { get => this.method; }

    public string FormACtion { get => this.action; }

    /// <summary>
    /// Gets the form keys.
    /// </summary>
    public IEnumerable<string> Keys
    {
        get
        {
            return this.keys.Keys;
        }
    }

    /// <summary>
    /// Gets the single string value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">more than one value is set for the specified key</exception>
    public string? GetStringValue(string key)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            return null;
        }

        if (values.Count == 0)
        {
            return null;
        }
        else if (values.Count == 1)
        {
            return values[0].Value;
        }
        else
        {
            throw new InvalidOperationException("Form has many values for key <" + key + ">");
        }
    }

    /// <summary>
    /// Gets all the string values associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IList<string> GetStringValues(string key)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            this.keys.Add(key, values = new WebFormValues());
        }

        return values.AsStringList();
        var list = new List<string>(values.Select(x => x.Value));
        return list;
    }

    /// <summary>
    /// Sets one string value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetStringValue(string key, string? value)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        if (value != null)
        {
            values.Clear();
            values.Add(new WebFormValue(value));
        }
        else
        {
            values.Clear();
        }
    }

    /// <summary>
    /// Adds one string value associated to the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddStringValue(string key, string value)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        values.Add(new WebFormValue(value));
    }

    /// <summary>
    /// Gets the single DateTime value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="format"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException">more than one value is set for the specified key</exception>
    /// <exception cref="FormatException"></exception>
    public DateTime? GetDateTimeValue(string key, string format, DateTimeKind kind)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            return null;
        }

        if (values.Count == 0)
        {
            return null;
        }
        else if (values.Count == 1)
        {
            return DateTime.ParseExact(values[0].Value, format, CultureInfo.InvariantCulture, DateTimeStyles.None).ChangeKind(kind);
        }
        else
        {
            throw new InvalidOperationException("Form has many values for key <" + key + ">");
        }
    }

    /// <summary>
    /// Sets a single DateTime value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    public void SetDateTimeValue(string key, DateTime? value, string format)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        if (value != null)
        {
            values.Add(new WebFormValue(value.Value.ToString(format, CultureInfo.InvariantCulture)));
        }
        else
        {
            values.Clear();
        }
    }

    /// <summary>
    /// Gets the single TimeSpan value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">more than one value is set for the specified key</exception>
    /// <exception cref="FormatException"></exception>
    public TimeSpan? GetTimeSpanValue(string key, string format)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            return null;
        }

        if (values.Count == 0)
        {
            return null;
        }
        else if (values.Count == 1)
        {
            return TimeSpan.ParseExact(values[0].Value, format, CultureInfo.InvariantCulture);
        }
        else
        {
            throw new InvalidOperationException("Form has many values for key <" + key + ">");
        }
    }

    /// <summary>
    /// Sets the single TimeSpan value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    public void SetTimeSpanValue(string key, TimeSpan? value, string format)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        if (value != null)
        {
            values.Add(new WebFormValue(value.Value.ToString(format, CultureInfo.InvariantCulture)));
        }
        else
        {
            values.Clear();
        }
    }

    /// <summary>
    /// Gets the single integer value associated to the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">more than one value is set for the specified key</exception>
    /// <exception cref="FormatException"></exception>
    public long? GetLongValue(string key)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            return null;
        }

        if (values.Count == 0)
        {
            return null;
        }
        else if (values.Count == 1)
        {
            return long.Parse(values[0].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        else
        {
            throw new InvalidOperationException("Form has many values for key <" + key + ">");
        }
    }

    /// <summary>
    /// Sets the single integer value associated to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetLongValue(string key, long? value)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        if (value != null)
        {
            values.Add(new WebFormValue(value.Value.ToString(CultureInfo.InvariantCulture)));
        }
        else
        {
            values.Clear();
        }
    }
    
    /// <summary>
    /// Gets the form data in query string encoded form.
    /// </summary>
    /// <returns></returns>
    public string GetFormData()
    {
        var builder = new StringBuilder();
        var sep = string.Empty;
        foreach (var key in this.keys)
        {
            if (key.Value.Count == 0)
            {
                
                builder.Append(sep);
                builder.Append(Uri.EscapeDataString(key.Key));
                builder.Append('=');
                sep = "&";
            }
            else
            {
                foreach (var value in key.Value)
                {
                    builder.Append(sep);
                    builder.Append(Uri.EscapeDataString(key.Key));
                    builder.Append('=');
                    builder.Append(Uri.EscapeDataString(value.Value));
                    sep = "&";
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Removes a key and the associated values. 
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        this.keys.Remove(key);
    }

    /// <summary>
    /// Removes the associated values to the specified key. 
    /// </summary>
    /// <param name="key"></param>
    public void Clear(string key)
    {
        if (this.keys.TryGetValue(key, out var values))
        {
            values.Clear();
        }
    }

    /// <summary>
    /// Enumerates all pairs of key and value. 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, string>> GetPairs()
    {
        foreach (var key in this.keys)
        {
            if (key.Value.Count == 0)
            {
                yield return new KeyValuePair<string, string>(key.Key, String.Empty);
            }
            else
            {
                foreach (var value in key.Value)
                {
                    yield return new KeyValuePair<string, string>(key.Key, value.Value);
                }
            }
        }
    }

    /// <summary>
    /// Loads url-encoded form data. 
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void LoadFormData(string data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var source = QueryHelpers.ParseQuery(data);
        foreach (var entry in source)
        {
            WebFormValues localValues;
            if (!this.keys.TryGetValue(entry.Key, out localValues))
            {
                localValues = new();
                this.keys.Add(entry.Key, localValues);
            }

            var inputValues = entry.Value;
            if (inputValues.Count == 0 || inputValues.Count == 1 && inputValues[0] == String.Empty)
            {
                if (!this.keys.ContainsKey(entry.Key))
                {
                }
            }
            else
            {
                foreach (var value in inputValues)
                {
                    this.SetStringValue(entry.Key, value);
                }
            }
        }
    }

    private sealed class WebFormValues : Collection<WebFormValue>
    {
        public IList<string> AsStringList()
        {
            var container = new WebFormValuesList(this);
            return container;
        }

        private class WebFormValuesList : Collection<string>
        {
            private readonly WebFormValues me;
            private bool isReady;

            public WebFormValuesList(WebFormValues me)
            {
                this.me = me;
                foreach (var item in me.Items)
                {
                    this.Add(item.Value);
                }

                this.isReady = true;
            }

            protected override void ClearItems()
            {
                base.ClearItems();

                if (this.isReady)
                {
                    this.me.Clear();
                }
            }

            protected override void InsertItem(int index, string item)
            {
                base.InsertItem(index, item);

                if (this.isReady)
                {
                    this.me.Insert(index, new WebFormValue(item));
                }
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);

                if (this.isReady)
                {
                    this.me.RemoveAt(index);
                }
            }

            protected override void SetItem(int index, string item)
            {
                throw new NotSupportedException();
                base.SetItem(index, item);
            }
        }
    }

    private sealed class WebFormValue
    {
        public WebFormValue(string value)
        {
            this.Value = value;
        }

        public string Value { get; init; }
    }
}
