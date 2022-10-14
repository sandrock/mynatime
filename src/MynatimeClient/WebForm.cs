
namespace Mynatime.Client;

using Microsoft.AspNetCore.WebUtilities;
using Mynatime.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

/// <summary>
/// Container for web form values. With utility methods. 
/// </summary>
public sealed class WebForm
{
    private readonly Dictionary<string, WebFormValues> keys = new Dictionary<string, WebFormValues>();

    public WebForm()
    {
    }

    public WebForm(params string[] keys)
    {
        foreach (var key in keys)
        {
            this.keys.Add(key, new WebFormValues());
        }
    }

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

    public void SetStringValue(string key, string? value)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        if (value != null)
        {
            values.Add(new WebFormValue(value));
        }
        else
        {
            values.Clear();
        }
    }

    public void AddStringValue(string key, string value)
    {
        if (!this.keys.TryGetValue(key, out WebFormValues? values))
        {
            values = new WebFormValues();
            this.keys.Add(key, values);
        }

        values.Add(new WebFormValue(value));
    }

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
            return long.Parse(values[0].Value, CultureInfo.InvariantCulture);
        }
        else
        {
            throw new InvalidOperationException("Form has many values for key <" + key + ">");
        }
    }

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

    public void Remove(string key)
    {
        this.keys.Remove(key);
    }

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

    private class WebFormValues : Collection<WebFormValue>
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

    private class WebFormValue
    {
        public WebFormValue(string value)
        {
            this.Value = value;
        }

        public string Value { get; init; }
    }
}
