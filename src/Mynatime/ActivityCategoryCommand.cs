
namespace Mynatime;

using Mynatime.Infrastructure;
using MynatimeClient;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public sealed class ActivityCategoryCommand : Command
{
    private readonly IManatimeWebClient manatimeWebClient;

    public ActivityCategoryCommand(IConsoleApp consoleApp, IManatimeWebClient manatimeWebClient)
        : base(consoleApp)
    {
        this.manatimeWebClient = manatimeWebClient;
    }

    public static string[] Args { get; } = new string[] { "cat", "cate", "categ", "category", "categories", };

    public bool DoRefresh { get; set; } = false;

    public string Search { get; set; }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ActivityCommand.Args);
    }

    public override bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command)
    {
        var i = -1;
        if (++i >= args.Length || !this.MatchArg(args[i]))
        {
            goto error;
        }

        if (++i >= args.Length || !ConsoleApp.MatchArg(args[i], Args))
        {
            goto error;
        }

        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null;
            if (ConsoleApp.MatchArg(arg, "--refresh", "refresh") || ConsoleApp.MatchShortArg(arg, "-r"))
            {
                this.DoRefresh = true;
            }
            else if (ConsoleApp.MatchArg(arg, "search") || ConsoleApp.MatchShortArg(arg, "-q", out value))
            {
                if (value != null)
                {
                    this.Search = value;
                }
                else if (nextArg != null)
                {
                    this.Search = nextArg;
                    i++;
                }
                else
                {
                    Console.WriteLine("Search command requires a search expression. ");
                }
            }
            else
            {
                goto error;
            }
        }

        ok:
        consumedArgs = args.Length;
        command = this;
        return true;

        error:
        consumedArgs = 0;
        command = null;
        return false;
    }

    public override async Task Run()
    {
        var profile = this.App.CurrentProfile;
        var store = profile.Data?.ActivityCategories;
        var existingItems = store.Items.ToList();
        var newItems = new List<MynatimeProfileDataActivityCategory>();
        var deletedItems = new List<MynatimeProfileDataActivityCategory>();

        var hasRefreshed = false;
        if (this.DoRefresh)
        {
            hasRefreshed = await this.Refresh(profile, existingItems, newItems, deletedItems);
        }

        if (profile == null)
        {
            throw new InvalidOperationException("No current profile. ");
        }

        if (store != null)
        {
            var allItems = store.Items.ToList();
            var items = allItems;
            if (this.Search != null)
            {
                items = this.SearchItems(allItems);
            }
            
            foreach (var item in items)
            {
                Console.WriteLine(item.ToString());
            }
        }

        if (hasRefreshed)
        {
            // TODO: this is not unit-testable :@
            Debug.Assert(profile.FilePath != null, "profile.FilePath != null");
            await profile.SaveToFile(profile.FilePath);
        }
    }

    private async Task<bool> Refresh(
        MynatimeProfile profile,
        List<MynatimeProfileDataActivityCategory> existingItems,
        List<MynatimeProfileDataActivityCategory> newItems,
        List<MynatimeProfileDataActivityCategory> deletedItems)
    {
        var profileData = profile.Data ?? new MynatimeProfileData();

        Console.WriteLine("Refreshing categories... ");
        // first try to get the page
        var page = await this.manatimeWebClient.GetNewActivityItemPage();
        if (page.Succeed)
        {
            // yeah!
        }
        else if (page.Errors?.Any(x => x.Code == "LoggedOut") ?? false)
        {
            // session expired: renew
            Console.WriteLine("  Renewing session... ");

            if (profile.LoginUsername == null || profile.LoginPassword == null)
            {
                throw new InvalidOperationException("Missing authentication information. ");
            }

            var loginPage = await this.manatimeWebClient.PrepareEmailPasswordAuthenticate();
            if (loginPage.Succeed)
            {
                var loginResultPage = await this.manatimeWebClient.EmailPasswordAuthenticate(profile.LoginUsername, profile.LoginPassword);
                if (loginResultPage.Succeed)
                {
                    page = await this.manatimeWebClient.GetNewActivityItemPage();
                    if (page.Succeed)
                    {
                        // yeah!
                    }
                    else
                    {
                        Console.WriteLine("Auto log-in failed: something went wrong 3. ");
                    }
                }
                else
                {
                    Console.WriteLine("Auto log-in failed: something went wrong 2. ");
                }
            }
            else
            {
                Console.WriteLine("Auto log-in failed: something went wrong 1. ");
            }
        }
        else
        {
            Console.WriteLine("Auto log-in failed: something went wrong 0. ");
        }

        profile.Cookies = this.manatimeWebClient.GetCookies();

        if (page.Succeed)
        {
            // add and update items 
            foreach (var category in page.Categories)
            {
                var match = existingItems.SingleOrDefault(x => x.Id == category.Id);
                if (match != null)
                {
                    match.LastUpdated = page.LoadTime;
                    category.Update(match, page.LoadTime);
                }
                else
                {
                    match = new MynatimeProfileDataActivityCategory(category.Id, category.DisplayName);
                    match.Created = page.LoadTime;
                    category.Update(match, page.LoadTime);
                    newItems.Add(match);
                }
            }

            // remove items
            foreach (var entry in existingItems)
            {
                var match = page.Categories.SingleOrDefault(x => x.Id == entry.Id);
                if (match != null)
                {
                    // okay
                }
                else
                {
                    deletedItems.Add(entry);
                    entry.Deleted = page.LoadTime;
                }
            }

            // insert new items
            Debug.Assert(profileData.ActivityCategories != null, "profileData.ActivityCategories != null");
            foreach (var entry in newItems)
            {
                profileData.ActivityCategories.Add(entry);
            }

            profileData.ActivityCategories.LastUpdated = page.LoadTime;

            return true;
        }
        else
        {
            Console.WriteLine(page.ToString());
            return false;
        }
    }

    private List<MynatimeProfileDataActivityCategory> SearchItems(List<MynatimeProfileDataActivityCategory> source)
    {
        var result = new List<MynatimeProfileDataActivityCategory>();
        
        // exact match search
        foreach (var item in source)
        {
            if (item.Id != null && item.Id.Equals(this.Search, StringComparison.OrdinalIgnoreCase))
            {
                result.AddIfAbsent(item);
            }
            
            if (item.Name != null && item.Name.Equals(this.Search, StringComparison.OrdinalIgnoreCase))
            {
                result.AddIfAbsent(item);
            }
        }
        
        // partial match search foreach (var item in source)
        foreach (var item in source)
        {
            if (item.Name != null && item.Name.Contains(this.Search, StringComparison.OrdinalIgnoreCase))
            {
                result.AddIfAbsent(item);
            }
        }

        return result;
    }
}
