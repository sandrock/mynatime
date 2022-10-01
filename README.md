
Mynatime
====================

Info
-------------

Project status:          **experimental** (**does not work yet**, see todo list)    
Contributions:           **open to contributions**: use issues and PRs    
Goal:                    **provide a fast GUI to update my activities into the service**.    
Relation to the service: this is an unofficial app.   
Build and run:           netcore6.0   

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
mynatime 
  [--config-directory ~/.config/mynatime] [--profile myname@company.com]
  command

commands:

  help:                            displays the help message
  profiles:                        list profiles
  profile add:                     adds a new profile
  activity category:               list the categories of activity
  activity category search <term>: searches the list the categories of activity
  activity category refresh:       refreshes the categories of activity
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






