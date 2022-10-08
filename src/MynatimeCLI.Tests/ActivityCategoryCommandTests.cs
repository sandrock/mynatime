
namespace Mynatime.CLI.Tests;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mynatime;
using Mynatime.Infrastructure;
using Mynatime.CLI.Tests.Resources;
using Mynatime.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class ActivityCategoryCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);

    public static IEnumerable<object[]> ValidInitialArgument()
    {
        yield return new object[] { "act", "cat", };
        yield return new object[] { "Activity", "Category", };
        yield return new object[] { "activities", "categories", };
    }

    public static IEnumerable<object[]> InvalidInitialArgument()
    {
        yield return new object[] { "act", "cet", };
        yield return new object[] { "category", "activity", };
        yield return new object[] { "activities", };
        yield return new object[] { "categories", };
    }

    [Theory, MemberData(nameof(ValidInitialArgument))]
    public void MatchArg_Yes(string arg0, string arg1)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.MatchArg(arg0);
        Assert.True(result);
        result = target.ParseArgs(app.Object, new string[] { arg0, arg1, }, out int consumedArgs, out Command? command);
        Assert.True(result);
    }

    [Theory, MemberData(nameof(InvalidInitialArgument))]
    public void MatchArg_No(params string[] args)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
    }

    [Fact]
    public void MatchArgs_List()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, new string[] { "activity", "category", }, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.DoRefresh);
        Assert.Null(target.Search);
    }

    [Fact]
    public void MatchArgs_Refresh()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, new string[] { "activity", "category", "refresh", }, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.DoRefresh);
        Assert.Null(target.Search);
    }

    [Fact]
    public void MatchArgs_Search1()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, new string[] { "activity", "category", "search", "hello", }, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.DoRefresh);
        Assert.Equal("hello", target.Search);
    }

    [Fact]
    public void MatchArgs_Search2()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, new string[] { "activity", "category", "search", "hello", "world", }, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.DoRefresh);
        Assert.Equal("hello world", target.Search);
    }

    [Fact]
    public async Task Run_Cached_NoData_Works()
    {
        var app = GetAppMock(true);
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        target.DoRefresh = false;
        await target.Run();
    }

    [Fact]
    public async Task Run_Cached_SomeData_Works()
    {
        var app = GetAppMock(true);
        var client = GetClientMock();
        app.Object.CurrentProfile.Data.ActivityCategories.Add(new MynatimeProfileDataActivityCategory("2", "yes"));
        app.Object.CurrentProfile.Data.ActivityCategories.Add(new MynatimeProfileDataActivityCategory("33", "no"));
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        target.DoRefresh = false;
        await target.Run();
    }

    [Fact]
    public async Task Run_Cached_Search_Works()
    {
        var app = GetAppMock(true);
        var client = GetClientMock();
        app.Object.CurrentProfile.Data.ActivityCategories.Add(new MynatimeProfileDataActivityCategory("2", "yes"));
        app.Object.CurrentProfile.Data.ActivityCategories.Add(new MynatimeProfileDataActivityCategory("33", "no"));
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        target.DoRefresh = false;
        target.Search = "yes";
        await target.Run();
    }

    [Fact(Skip = "cannot mock the Run method (yet)")]
    public async Task Run_Refresh_Works()
    {
        var app = GetAppMock(true);
        var client = GetClientMock();
        var page = new NewActivityItemPage();
        page.IsEmptyCategoryAllowed = true;
        page.LoadTime = new DateTime(2022, 9, 18, 10, 7, 0, DateTimeKind.Utc);
        page.Categories = new List<SelectItem>();
        page.Categories.Add(new SelectItem() { Id = "1", DisplayName = "some category", Index = 1, });
        page.Token = "xxxxxxxxxxxxx";
        client.Setup(x => x.GetCookies()).Returns(new JArray());
        client.Setup(x => x.GetNewActivityItemPage()).Returns(Task.FromResult(page)).Verifiable();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        target.DoRefresh = true;
        await target.Run();
        client.Verify(x => x.GetNewActivityItemPage(), () => Times.Once());
    }

    [Fact]
    public async Task SearchItems_BestMatch_InterneCompany()
    {
        var app = GetAppMock(true);
        var client = GetClientMock();
        ActivityTesting.PopulateCategories1(app.Object.CurrentProfile.Data.ActivityCategories);
        var search = "company interne";
        var searchResult = await ActivityCategoryCommand.SearchItems(app.Object.CurrentProfile.Data.ActivityCategories.Items.ToList(), search, true);
        var result = searchResult.Select(x => x.Item).ToList();

        Assert.Collection(
            result,
            x => Assert.Equal("MyCompany-Interne", x.Name));
    }

    [Fact]
    public async Task SearchItems_OpenMatch_TinyDiff()
    {
        var source = new MynatimeProfileDataActivityCategories(new JObject());
        ActivityTesting.PopulateCategories1(source);
        var input = "prospect";
        var searchResult = await ActivityCategoryCommand.SearchItems(source.Items.ToList(), input, false);
        var result = searchResult.Select(x => x.Item).ToList();
        Assert.Contains(result, x => "Prospects" == x.Name);
        Assert.Contains(result, x => "Project ASDFG" == x.Name);
        Assert.Contains(result, x => "Project TYUI3-Branch2" == x.Name);
    }

    [Fact]
    public async Task SearchItems_BestMatch_TinyDiff()
    {
        var source = new MynatimeProfileDataActivityCategories(new JObject());
        ActivityTesting.PopulateCategories1(source);
        var input = "prospect";
        var searchResult = await ActivityCategoryCommand.SearchItems(source.Items.ToList(), input, true);
        var result = searchResult.Select(x => x.Item).ToList();
        Assert.Collection(
            result,
            x => Assert.Equal("Prospects", x.Name));
    }

    private Mock<IManatimeWebClient> GetClientMock()
    {
        var mock = this.mocks.Create<IManatimeWebClient>();
        return mock;
    }

    private Mock<IConsoleApp> GetAppMock(bool withProfile = false)
    {
        var mock = this.mocks.Create<IConsoleApp>();

        var profile = new MynatimeProfile();
        if (withProfile)
        {
            mock.SetupGet(x => x.CurrentProfile).Returns(profile);
        }

        return mock;
    }
}
