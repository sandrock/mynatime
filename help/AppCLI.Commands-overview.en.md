
[Index](AppCLI.0Index.en.md) - [Back: Installing from sources](AppCLI.Install-from-sources.en.md) - [Next: Authenticate with service](AppCLI.Authenticate.en.md)

---

Commands overview - Mynatime CLI app documentation
====================================

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



---

[Index](AppCLI.0Index.en.md) - [Back: Installing from sources](AppCLI.Install-from-sources.en.md) - [Next: Authenticate with service](AppCLI.Authenticate.en.md)
