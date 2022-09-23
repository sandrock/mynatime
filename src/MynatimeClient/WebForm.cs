
namespace MynatimeClient;

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

    public DateTime? GetDateTimeValue(string key, string format)
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
            return DateTime.ParseExact(values[0].Value, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
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
    
    public string GetFormData()
    {
        var builder = new StringBuilder();
        var sep = string.Empty;
        foreach (var key in this.keys)
        {
            builder.Append(sep);
            foreach (var value in key.Value)
            {
                builder.Append(Uri.EscapeDataString(key.Key));
                builder.Append('=');
                builder.Append(Uri.EscapeDataString(value.Value));
                sep = "&";
            }
        }

        return builder.ToString();
    }

    private class WebFormValues : Collection<WebFormValue>
    {
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
