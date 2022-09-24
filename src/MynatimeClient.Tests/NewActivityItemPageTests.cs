
namespace MynatimeClient.Tests;

using System;
using Xunit;

public class NewActivityItemPageTests
{
    [Fact]
    public void GetFormData_DateAndTime()
    {
        var page = new NewActivityItemPage();
        page.ActivityId = "123456";
        page.DateStart = new DateTime(2022, 9, 20, 0, 0, 0, DateTimeKind.Local);
        page.DateEnd = new DateTime(2022, 9, 20, 0, 0, 0, DateTimeKind.Local);
        page.InAt = new TimeSpan(22, 21, 0);
        page.OutAt = new TimeSpan(22, 22, 0);
        page.Comment = "comment\r\ncomment comment\r\ncomment";
        page.Token = "dfghjkfghjrftgy_h-uji";
        var result = page.GetFormData();
        Assert.Equal("create%5Btask%5D=123456&create%5BdateStart%5D=2022-09-20&create%5BdateEnd%5D=2022-09-20&create%5BinAt%5D=22%3A21&create%5BoutAt%5D=22%3A22&create%5Bduration%5D=&create%5Bcomment%5D=comment%0D%0Acomment%20comment%0D%0Acomment&submitAdvanced=&create%5B_token%5D=dfghjkfghjrftgy_h-uji", result);
    }

    [Fact]
    public void GetFormData_DateAndDuration()
    {
        var page = new NewActivityItemPage();
        page.ActivityId = "123456";
        page.DateStart = new DateTime(2022, 9, 20, 0, 0, 0, DateTimeKind.Local);
        page.DateEnd = new DateTime(2022, 9, 20, 0, 0, 0, DateTimeKind.Local);
        page.Duration = "1.5";
        page.Comment = "comment\r\ncomment comment\r\ncomment";
        page.Token = "dfghjkfghjrftgy_h-uji";
        var result = page.GetFormData();
        Assert.Equal("create%5Btask%5D=123456&create%5BdateStart%5D=2022-09-20&create%5BdateEnd%5D=2022-09-20&create%5BinAt%5D=&create%5BoutAt%5D=&create%5Bduration%5D=1.5&create%5Bcomment%5D=comment%0D%0Acomment%20comment%0D%0Acomment&submitAdvanced=&create%5B_token%5D=dfghjkfghjrftgy_h-uji", result);
    }
}
