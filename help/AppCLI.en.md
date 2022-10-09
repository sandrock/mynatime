
Mynatime CLI app documentation
====================================

Installing from sources
----------------------------

### Install from sources

1. Clone the repository
2. Build it  
```bash
cd src
dotnet build
```
3. Alias the CLI app and verify  
```bash
alias "mynatime=/home/me/dd/github/sandrock/mynatime/src/Mynatime/bin/Debug/net6.0/Mynatime.CLI"
which mynatime
mynatime help
```
4. Add the alias to your `~/.bashrc` 

### Update from sources

1. Update sources  
```bash
git pull
```
2. Build it  
```bash
cd src
dotnet build
```


Mynatime CLI commands
----------------------------

The app is built around command line instructions. Each command has a long name and a short name. The long name is more expressive, the short name is faster to type. Use the command you want.

The app will help you work offline with the service.  Some commands will do things locally and  some other commands will push your changes to the service. 

The first command to learn is the `help` command. It will list the available commands.

```bash
mynatime help
```

> MynatimeCLI  Mynatime help  
>   
> Usage: `mynatime` `[app options]` `[command]` `[command options]`  
>   
> App options:  
>   
>   `--config-directory <directory>`   specifies the directory to open profiles from  
>   `--profile <profile>`              specifies the profile to use  
>   `-p <profile>`  
>   
> Commands:  
>   
>   HelpCommand  
>   `help`                             displays help  
>   
>   ProfileListCommand  
>   `pro list`                         lists user profiles  
>   
>   ProfileAddCommand  
>   `pro add`                          adds user profiles  
>   
>   ActivityCategoryCommand  
>   `act cat`                          lists activity categories  
>   `act cat refresh`                  updates activity categories from service  
>   `act cat search <q>`               searches categories  
>   
>   Add activity   
>   `act add [date] <duration> [category]`                         adds an activity  
>   `act add <date> <time-start> [date-end] <time-end> [category]` adds an activity   
> 
>   Activity tracker  
>   `act start [time] [category]`      starts an activity  
>   `act stop [time] [category]`       stops  an activity   
>   `act status`                       lists current activities  
>   
>   StatusCommand  
>   `status`                           lists pending changes  
>   
>   CommitCommand  
>   `commit`                           saves pending changes  
> 

After updating the app, execute the `help` command to see the new commands available.


Authenticate
----------------------------

The app supports multiple user profiles. When you set up a user profile, a JSON file will be saved at location `~/.config/mynatime/profile.xxxx.json`. You typically need only one profile for most scenarios. 

Use one of these commands to look at the available user profiles. 

```bash
mynatime profiles
mynatime pro
```

> MynatimeCLI  
No profiles found.

You can add a profile using one of these commands.  
NOTE: your username and password will be saved in the profile file to help the app publish information to the service without asking the password each time.

```bash
mynatime profile add
mynatime pro add
```

> MynatimeCLI   
Creating a new profile. Please authenticate.   
Email address> test@test.com  
Password>      ***************  
Processing... OK.   
Profile saved to: /home/me/.config/mynatime/profile.20221009T121214008Z.json

List the profiles again to verify.


Add an activity
----------------------------

First, you have to pull the list of activity categories from the service.

```bash
mynatime activity category refresh
mynatime act cat refresh
```

> MynatimeCLI   
> Refreshing categories...  
Renewing session...  
ActCategory 51615 &lt;Out of office - planned&gt;   
ActCategory 51077 &lt;MyCompany - Internal&gt;  
ActCategory 51432 &lt;MyCompany - Demos&gt;  
ActCategory 51712 &lt;Project 123&gt;  
ActCategory 51402 &lt;Internal System Administration&gt;      
ActCategory 51556 &lt;Production System Administration&gt;  
ActCategory 51112 &lt;Project 789&gt;  
> ...

You can display categories at any time:

```bash
mynatime activity category
mynatime act cat
```

If a category is created by your manager, you will need to execute the refresh command again.

You can search a category:

```bash
mynatime activity category search project something
mynatime act cat search project something
```

Now let's add an activity for the current day, using start time (08:30) and end time (11:50) for project "Internal".

```bash
mynatime act add 0830 1150 Internal
```

Now let's add an activity for yesterday (2022-09-15), using start time (08:30) and end time (11:50) for project "Internal".

```bash
mynatime act add 2022-09-15 0830 1150 Internal
```

You can also use duration (2.7 hours) instead of times (for "Project 123").

```bash
mynatime act add 2022-09-15 2.7 Project-123
```

All this is only saved on your computer. You can review what you created:

```bash
mynatime status
```

If it looks okay, you can publish to the service:

```bash
mynatime commit
```



