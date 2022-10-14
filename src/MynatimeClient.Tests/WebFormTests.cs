
namespace Mynatime.Client.Tests;

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class WebFormTests
{
    [Fact]
    public void GetSetStringValue()
    {
        var input = "John";
        var form = new SampleForm1();
        
        // set values
        form.Firstname = input;
        
        // get values
        Assert.Equal(input, form.Firstname);
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("firstname", x));
        Assert.Equal("firstname=John", form.Form.GetFormData());
    }

    [Fact]
    public void AddStringValue()
    {
        var form = new WebForm();
        
        // set values
        form.AddStringValue("a", "aa");
        form.AddStringValue("a", "aaa");
        
        // get values
        Assert.Collection(
            form.GetStringValues("a"),
            x => Assert.Equal("aa", x),
            x => Assert.Equal("aaa", x));
        
        // verify
        Assert.Collection(form.Keys, x => Assert.Equal("a", x));
        Assert.Equal("a=aa&a=aaa", form.GetFormData());
    }

    [Fact]
    public void GetSetDateValue_Utc()
    {
        var kind = DateTimeKind.Utc;
        var input = new DateTime(2012,12,16,13,37,42, kind);
        var form = new SampleForm1();
        
        // set values
        form.DateCreated = input;

        // get values
        Assert.Equal(input.ToPrecision(DateTimePrecision.Day), form.DateCreated.Value.ToPrecision(DateTimePrecision.Day));

        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("created", x));
        Assert.Equal("created=2012-12-16", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetDateValue_Local()
    {
        var kind = DateTimeKind.Local;
        var input = new DateTime(2012,12,16,13,37,42, kind);
        var form = new SampleForm1();
        
        // set values
        form.DateCreated = input;
        
        // get values
        Assert.Equal(input.ToPrecision(DateTimePrecision.Day), form.DateCreated.Value.ToPrecision(DateTimePrecision.Day));
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("created", x));
        Assert.Equal("created=2012-12-16", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetDateValue_Other()
    {
        var kind = DateTimeKind.Unspecified;
        var input = new DateTime(2012,12,16,0,0,0, kind);
        var form = new SampleForm1();
        
        // set values
        form.DateCreated = input;
        
        // get values
        Assert.Equal(input.ToPrecision(DateTimePrecision.Day), form.DateCreated.Value.ToPrecision(DateTimePrecision.Day));
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("created", x));
        Assert.Equal("created=2012-12-16", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetDateValue_Null()
    {
        var kind = DateTimeKind.Unspecified;
        var input = default(DateTime?);
        var form = new SampleForm1();
        
        // get values
        Assert.Null(form.DateCreated);

        // set values
        form.DateCreated = input;
        
        // get values
        Assert.Null(form.DateCreated);
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("created", x));
        Assert.Equal("created=", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetTimeValue()
    {
        var input = new TimeSpan(0, 13, 36, 42, 123);
        var form = new SampleForm1();
        
        // set values
        form.TimeCreated = input;
        
        // get values
        Assert.Equal(new TimeSpan(0, 13, 36, 42, 0), form.TimeCreated.Value);
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("createdTime", x));
        Assert.Equal("createdTime=13%3A36%3A42", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetDateTimeValue_Other()
    {
        var kind = DateTimeKind.Unspecified;
        var input = new DateTime(2012,12,16,13, 36, 42, kind);
        var form = new SampleForm1();
        
        // set values
        form.DateTimeUpdated = input;
        
        // get values
        Assert.Equal(input, form.DateTimeUpdated.Value);
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("updated", x));
        Assert.Equal("updated=2012-12-16T13-36-42", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetIntegerValue()
    {
        var input = long.MaxValue - 10;
        var form = new SampleForm1();
        
        // set values
        form.UserId = input;
        
        // get values
        Assert.Equal(input, form.UserId.Value);
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("user_id", x));
        Assert.Equal("user_id=9223372036854775797", form.Form.GetFormData());
    }

    [Fact]
    public void GetSetStringValues()
    {
        var input = "John";
        var form = new SampleForm1();
        
        // set values
        form.Roles.Add("admin");
        form.Roles.Add("user");

        // get values
        Assert.Collection(
            form.Roles,
            x => Assert.Equal("admin", x),
            x => Assert.Equal("user", x));
        
        // verify
        Assert.Collection(form.Form.Keys, x => Assert.Equal("roles", x));
        Assert.Equal("roles=admin&roles=user", form.Form.GetFormData());
    }

    [Fact]
    public void Ctor()
    {
        // not settings keys in the constructor will generate data in the order of usage
        var form = new WebForm();
        form.SetStringValue("x2", "value");
        form.SetStringValue("x1", "value");
        Assert.Collection(
            form.Keys,
            x => Assert.Equal("x2", x),
            x => Assert.Equal("x1", x));
        Assert.Equal("x2=value&x1=value", form.GetFormData());
    }
    
    [Fact]
    public void Ctor_WithKeys()
    {
        // settings keys in the constructor will help get data in the desired order
        var form = new WebForm("x1", "x2", "x3", "x4");
        form.SetStringValue("x2", "value");
        form.SetStringValue("x1", "value");
        form.Remove("x4");
        Assert.Collection(
            form.Keys,
            x => Assert.Equal("x1", x),
            x => Assert.Equal("x2", x),
            x => Assert.Equal("x3", x));
        Assert.Equal("x1=value&x2=value&x3=", form.GetFormData());
    }

    [Fact]
    public void GetPairs()
    {
        var form = new WebForm("a", "b", "c");
        form.SetStringValue("a", "aaa");
        form.AddStringValue("b", "bb");
        form.AddStringValue("b", "bbb");
        var result = form.GetPairs();
        Assert.Collection(
            result,
            x => { Assert.Equal("a", x.Key); Assert.Equal("aaa", x.Value); },
            x => { Assert.Equal("b", x.Key); Assert.Equal("bb", x.Value); },
            x => { Assert.Equal("b", x.Key); Assert.Equal("bbb", x.Value); },
            x => { Assert.Equal("c", x.Key); Assert.Equal(string.Empty, x.Value); });
    }

    [Fact]
    public void LoadFormData_BasicTest()
    {
        var form = new WebForm();
        form.LoadFormData("x1=value&x2=value&x3=");
        Assert.Collection(
            form.Keys,
            x => Assert.Equal("x1", x),
            x => Assert.Equal("x2", x),
            x => Assert.Equal("x3", x));
        Assert.Collection(
            form.GetPairs(),
            x => { Assert.Equal("x1", x.Key); Assert.Equal("value", x.Value); },
            x => { Assert.Equal("x2", x.Key); Assert.Equal("value", x.Value); },
            x => { Assert.Equal("x3", x.Key); Assert.Empty(x.Value); });
    }

    [Fact]
    public void LoadFormData_SampleForm1_Empty()
    {
        string formData;
        {
            var data1 = new SampleForm1();
            formData = data1.Form.GetFormData();
        }

        var data = new SampleForm1();
        data.Form.LoadFormData(formData);
        Assert.Null(data.Firstname);
        Assert.Empty(data.Roles);
        Assert.Null(data.DateCreated);
        Assert.Null(data.TimeCreated);
        Assert.Null(data.UserId);
        Assert.Null(data.DateTimeUpdated);
    }

    [Fact]
    public void LoadFormData_SampleForm1_Emptied()
    {
        string formData;
        {
            var data1 = new SampleForm1();
            data1.Firstname = null;
            data1.Roles.Add("me");
            data1.Roles.Clear();
            data1.DateCreated = null;
            data1.TimeCreated = null;
            data1.UserId = null;
            data1.DateTimeUpdated = null;
            formData = data1.Form.GetFormData();
        }

        var data = new SampleForm1();
        data.Form.LoadFormData(formData);
        Assert.Null(data.Firstname);
        Assert.Empty(data.Roles);
        Assert.Null(data.DateCreated);
        Assert.Null(data.TimeCreated);
        Assert.Null(data.UserId);
        Assert.Null(data.DateTimeUpdated);
    }

    [Fact]
    public void LoadFormData_SampleForm1_Filled()
    {
        string formData;
        var someDate = new DateTime(2012, 12, 16, 0, 0, 0, DateTimeKind.Local);
        {
            var data1 = new SampleForm1();
            data1.Firstname = "Anastasia";
            data1.Roles.Add("me");
            data1.DateCreated = someDate;
            data1.TimeCreated = null;
            data1.UserId = 7L;
            data1.DateTimeUpdated = someDate;
            formData = data1.Form.GetFormData();
        }

        var data = new SampleForm1();
        data.Form.LoadFormData(formData);
        Assert.Equal("Anastasia", data.Firstname);
        Assert.Collection(data.Roles, x => Assert.Equal("me", x));
        Assert.Equal(someDate, data.DateCreated);
        Assert.Null(data.TimeCreated);
        Assert.Equal(7L, data.UserId);
        Assert.Equal(someDate, data.DateTimeUpdated);
    }

    public class SampleForm1
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string TimeFormat = "hh\\:mm\\:ss";
        public const string DateTimeFormat = "yyyy-MM-ddTHH-mm-ss";

        private readonly WebForm form = new WebForm();

        public WebForm Form { get => this.form; }

        public string? Firstname
        {
            get { return this.form.GetStringValue("firstname"); }
            set { this.form.SetStringValue("firstname", value); }
        }

        public DateTime? DateCreated
        {
            get { return this.form.GetDateTimeValue("created", DateFormat, DateTimeKind.Local); }
            set { this.form.SetDateTimeValue("created", value, DateFormat); }
        }

        public TimeSpan? TimeCreated
        {
            get { return this.form.GetTimeSpanValue("createdTime", TimeFormat); }
            set { this.form.SetTimeSpanValue("createdTime", value, TimeFormat); }
        }

        public DateTime? DateTimeUpdated
        {
            get { return this.form.GetDateTimeValue("updated", DateTimeFormat, DateTimeKind.Local); }
            set { this.form.SetDateTimeValue("updated", value, DateTimeFormat); }
        }

        public long? UserId
        {
            get { return this.form.GetLongValue("user_id"); }
            set { this.form.SetLongValue("user_id", value); }
        }

        public IList<string> Roles
        {
            get { return this.form.GetStringValues("roles"); }
        }
    }
}
