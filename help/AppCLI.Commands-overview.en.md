
---
title: Commands overview
parent: Documentation
nav_order: 2
---

# Commands overview

The app is built around command line instructions. Each command has a long name and a short name. The long name is more expressive, the short name is faster to type. Use the command you want.

The app will help you work offline with the service.  Some commands will do things locally and  some other commands will push your changes to the service.

The first command to learn is the `help` command. It will list the available commands.

```bash
m help
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
>   Help  
>   `help`                             displays help
>
>   Profiles  
>   `pro list`                         lists user profiles  
>   `pro add`                          adds a user profile (interactive)  
>   `pro status`                       displays current profile status
>
>   Activity categories  
>   `act cat`                          lists activity categories  
>   `act cat refresh`                  updates activity categories from service  
>   `act cat search <q>`               searches categories  
>   `act cat alias <name> <alias>`     creates an alias for a category  
>
>   Add activity  
>   `act add [date] <duration> [category]`                         adds an activity  
>   `act add <date> <time-start> [date-end] <time-end> [category]` adds an activity
>
>   Activity tracker  
>   `act start [date] [time] [category]`      starts an activity  
>   `act stop  [date] [time] [category]`      stops an activity  
>   `act status`                              lists tracked activities  
>   `act events`                              lists raw start/stop events  
>   `act clear`                               removes all events
>
>   Review and commit  
>   `status`                           lists all pending changes  
>   `commit`                           sends pending changes to the service
>
>   App options  
>   `--version`, `-V`                  prints the current version
>

After updating the app, execute the `help` command to see the new commands available.

If the `m` command is not available, please review [your mynatime installation](AppCLI.Install-from-sources.en.md).
