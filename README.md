
Mynatime
====================

Info
-------------

|||
|--|--|
| Project status          | **beta test** (collecting user feedback)                          |
| Goal                    | **provide a fast GUI to update my activities into the service**.  |
| Relation to the service | unofficial app, use at your own risk                              |
| CLI app status          | **beta test** (collecting user feedback)                          |
| GUI app status          | **experimental**                                                  |
| Build and run           | net6.0                                                            |
| License                 | GNU General Public License Version 3                              |
| Contributions           | **open to contributions**: use issues and PRs                     |
| Main branch             | [![.NET](https://github.com/sandrock/mynatime/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sandrock/mynatime/actions/workflows/dotnet.yml) |

See also: [TODO list](TODO.md)

Why use this?

- some use cases provide better ergonomy
- offline usage of service
- simple time tracking utility


Building from sources
-------------

Run apps:

```
dotnet run --project src/Mynatime/Mynatime.csproj
dotnet run --project src/MynatimeGUI/MynatimeGUI.csproj
```

Or open the `src` directory in Visual Studio, Rider...


CLI app [📖](help/AppCLI.0Index.en.md)
-------------

The CLI is meant for offline usage. Inspired by the [git stage](https://git-scm.com/book/en/v2/Git-Basics-Recording-Changes-to-the-Repository), you can prepare changes and commit them when online. 

[📖 Full mynatime CLI app documentation](help/AppCLI.0Index.en.md). Command usage:

```
Usage: mynatime [app options] [command] [command options]

App options:

  --config-directory <directory>   specifies the directory to open profiles from
  --profile <profile>              specifies the profile to use
  -p <profile>

Commands:

  ## ProfileListCommand
  pro list                         lists user profiles

  ## ProfileAddCommand
  pro add                          adds user profiles

  ## ActivityCategoryCommand
  act cat                          lists activity categories
  act cat refresh                  updates activity categories from service
  act cat search <q>               searches categories

  ## Activity tracker
  act start [time] [category]             starts an activity
  act stop [time] [category]              stops  an activity
  act status                              lists current activities
  act clear                               removes all events
  act start/stop [date] [time] [category] you can specify a date

  ## Add activity
  act add [date] <duration> [category]                         adds an activity
  act add <date> <time-start> [date-end] <time-end> [category] adds an activity

  ## StatusCommand
  status                           lists pending changes

  ## CommitCommand
  commit                           send pending changes to service
```

See also: [TODO list](TODO.md) for the list of planned commands. 


GUI app
-------------

This is experimental. Does not work yet. 


Contribute
-------------

See issues in github.

Feel free to fork and enhance.


Resources
-------------

https://docs.avaloniaui.net/  


