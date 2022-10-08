
Mynatime
====================

Info
-------------

|||
|--|--|
| Project status          | **experimental** (some things work, see todo list)                |
| Goal                    | **provide a fast GUI to update my activities into the service**.  |
| Relation to the service | unofficial app, use at your own risk                              |
| Build and run           | netcore6.0                                                        |
| License                 | GNU General Public License Version 3                              |
| Contributions           | **open to contributions**: use issues and PRs                     |

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


CLI app
-------------

The CLI is meant for offline usage. Inspired by the [git stage](https://git-scm.com/book/en/v2/Git-Basics-Recording-Changes-to-the-Repository), you can prepare changes and commit them when online. 

Command usage:

```
Usage: mynatime [app options] [command] [command options]

App options:

  --config-directory <directory>   specifies the directory to open profiles from
  --profile <profile>              specifies the profile to use
  -p <profile>

Commands:

  ## HelpCommand
  help                 displays help

  ## ProfileListCommand
  pro list             lists user profiles

  ## ProfileAddCommand
  pro add              adds user profiles

  ## ActivityCategoryCommand
  act cat              lists activity categories
  act cat refresh      updates activity categories from service
  act cat search <q>   searches categories

  ## Add activity
  act add [date] <duration> [category]                         adds an activity
  act add <date> <time-start> [date-end] <time-end> [category] adds an activity

  ## Activity tracker
  act start [time] [category]     starts an activity
  act stop [time] [category]      stops  an activity
  act status                      lists current activities

  ## StatusCommand
  status               lists pending changes

  ## CommitCommand
  commit               saves pending changes
```

See also: [TODO list](TODO.md) for the list of planned commands. 


GUI app
-------------

...



Contribute
-------------

...


Resources
-------------

https://docs.avaloniaui.net/  






