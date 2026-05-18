---
title: Home
nav_order: 1
---

Mynatime
====================

Info
-------------

||                                                                                                                                                              |
|--|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Project status          | **beta test** (collecting user feedback)                                                                                                                     |
| Goal                    | **provide a fast CLI to manage activities in the time tracking service**.                                                                                    |
| Relation to the service | unofficial CLI and GUI app for Manatime, use at your own risk                                                                                                |
| CLI app status          | **beta test** (collecting user feedback)                                                                                                                     |
| GUI app status          | **experimental**                                                                                                                                             |
| Build and run           | net8.0                                                                                                                                                       |
| License                 | GNU General Public License Version 3                                                                                                                         |
| Contributions           | **open to contributions**: use issues and PRs                                                                                                                |
| CI                      | [![CI](https://github.com/sandrock/mynatime/actions/workflows/ci.yml/badge.svg)](https://github.com/sandrock/mynatime/actions/workflows/ci.yml)              |

Why use this?

- offline-first: prepare changes locally and push them when online
- git-style workflow: stage activities, review them, commit
- faster than the web UI for common operations
- simple time tracking with start/stop events


Install
-------------

**Linux** — one-liner, installs .NET if needed:

```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | bash
```

**Windows** — open PowerShell:

```powershell
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex
```

Both install `mynatime` and `m` commands and write a `mynatime-update` command for future updates.

[📖 Full install documentation](help/AppCLI.Install-from-sources.en.md)


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
  --version, -V                    prints the current version

Commands:

  ## Profiles
  pro list                         lists user profiles
  pro add                          adds a user profile (interactive)

  ## Activity categories
  act cat                          lists activity categories
  act cat refresh                  updates activity categories from service
  act cat search <q>               searches categories

  ## Activity tracker
  act start [time] [category]             starts an activity
  act stop  [time] [category]             stops an activity
  act status                              lists tracked activities
  act events                              lists raw start/stop events
  act clear                               removes all events
  act start/stop [date] [time] [category] you can specify a date

  ## Add activity
  act add [date] <duration> [category]                         adds an activity
  act add <date> <time-start> [date-end] <time-end> [category] adds an activity

  ## Review and commit
  status                           lists all pending changes
  commit                           sends pending changes to the service
```

See also: [TODO list](TODO.md) for planned commands.


Building from sources
-------------

```bash
dotnet run --project src/Mynatime/Mynatime.csproj
dotnet run --project src/MynatimeGUI/MynatimeGUI.csproj
```

Or open the `src` directory in Visual Studio, Rider...


GUI app
-------------

Experimental. Does not work yet.


Contribute
-------------

See issues on GitHub. Feel free to fork and enhance.


Resources
-------------

https://docs.avaloniaui.net/
