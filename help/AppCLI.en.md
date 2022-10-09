
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
> 

Authenticate
----------------------------

The app supports multiple user profiles. When you set up a user profile, a JSON file will be saved at location `~/.config/mynatime/profile.xxxx.json`. 

Use one of this commands to look at the available user profiles. 

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
Email address> test@test.com  
Password>      ***************  
Profile saved to: /home/me/.config/mynatime/profile.20221009T121214008Z.json

List the profiles again to verify.


