﻿
namespace MynatimeClient.Tests;

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
            get { return this.form.GetDateTimeValue("created", DateFormat); }
            set { this.form.SetDateTimeValue("created", value, DateFormat); }
        }

        public TimeSpan? TimeCreated
        {
            get { return this.form.GetTimeSpanValue("createdTime", TimeFormat); }
            set { this.form.SetTimeSpanValue("createdTime", value, TimeFormat); }
        }

        public DateTime? DateTimeUpdated
        {
            get { return this.form.GetDateTimeValue("updated", DateTimeFormat); }
            set { this.form.SetDateTimeValue("updated", value, DateTimeFormat); }
        }

        public long? UserId
        {
            get { return this.form.GetLongValue("user_id"); }
            set { this.form.SetLongValue("user_id", value); }
        }
    }
}
