
Mynatime
====================

Info
-------------

Project status:          experimental    
Contributions:           open to contributions, contact me    
Goal:                    provide a fast GUI to update my activities into manatime.    
Relation to the service: this is an unofficial app.   
Build and run:           dotnet core 6   

See also: [TODO list](TODO.md)


Building from sources
-------------

```
dotnet run --project src/Mynatime/Mynatime.csproj
dotnet run --project src/MynatimeGUI/MynatimeGUI.csproj
```



CLI app
-------------

Authenticate:

```bash
mynatime +profile [email]
```

Verify:

```bash
mynatime profiles
mynatime profile check
```

Activity categories:

```bash
mynatime activity categories
```

Add activity items and commit:

```bash
mynatime +act category123 2.5     # today   I did 2h 30min of Category123
mynatime +act 2022-04-16 4 cat5   # at date I did 4h       of cat5
mynatime +act 0855 1215 internal  # today, from 0855 to 1215, I was on internal
mynatime status                   # review   pending changes
mynatime commit                   # save all pending changes
```

Command usage:

```
mynatime 
  [--config-directory ~/.config/mynatime] [--profile myname@company.com]
  command

commands:

  profile[s]: list profiles

```



GUI app
-------------





Contribute
-------------

...


Resources
-------------

https://docs.avaloniaui.net/  






