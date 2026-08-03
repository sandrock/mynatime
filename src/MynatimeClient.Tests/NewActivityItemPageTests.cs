
namespace Mynatime.Client.Tests;

using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using System;
using Xunit;

public class NewActivityItemPageTests
{
    [Fact]
    public void SaveFailedReason_RoundTrips()
    {
        var reason = "Ce collaborateur n'est pas autorise a utiliser cette activite.";
        var page = new NewActivityItemPage();
        page.ActivityId = "123456";
        page.DateStart = new DateTime(2022, 9, 20, 0, 0, 0, DateTimeKind.Local);
        page.DateEnd = page.DateStart;
        page.InAt = new TimeSpan(9, 0, 0);
        page.OutAt = new TimeSpan(10, 0, 0);
        page.Comment = "hello";
        page.SaveFailedReason = reason;

        var item = page.ToTransactionItem(null, DateTime.UtcNow);
        var reloaded = (NewActivityItemPage)MynatimeProfileTransactionManager.Default.GetInstanceOf(item);

        Assert.Equal(reason, reloaded.SaveFailedReason);
        Assert.Equal("hello", reloaded.Comment);
    }

    [Fact]
    public void CommentForCommit_IsNotPersisted()
    {
        var page = new NewActivityItemPage();
        page.ActivityId = "123456";
        page.Comment = "clean";
        page.CommentForCommit = "clean\n\n-- \nMNTM;T=x;M=y;C=1";

        var formData = page.GetFormData();
        Assert.DoesNotContain("MNTM", formData);

        var item = page.ToTransactionItem(null, DateTime.UtcNow);
        var reloaded = (NewActivityItemPage)MynatimeProfileTransactionManager.Default.GetInstanceOf(item);

        Assert.Equal("clean", reloaded.Comment);
        Assert.Null(reloaded.CommentForCommit);
    }

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
        Assert.Equal("create%5Buser%5D=&create%5Btask%5D=123456&create%5BdateStart%5D=20%2F09%2F2022&create%5BdateEnd%5D=20%2F09%2F2022&create%5BinAt%5D=22%3A21&create%5BoutAt%5D=22%3A22&create%5Bduration%5D=&create%5Bcomment%5D=comment%0D%0Acomment%20comment%0D%0Acomment&submitAdvanced=&create%5B_token%5D=dfghjkfghjrftgy_h-uji", result);
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
        Assert.Equal("create%5Buser%5D=&create%5Btask%5D=123456&create%5BdateStart%5D=20%2F09%2F2022&create%5BdateEnd%5D=20%2F09%2F2022&create%5BinAt%5D=&create%5BoutAt%5D=&create%5Bduration%5D=1.5&create%5Bcomment%5D=comment%0D%0Acomment%20comment%0D%0Acomment&submitAdvanced=&create%5B_token%5D=dfghjkfghjrftgy_h-uji", result);
    }
}
