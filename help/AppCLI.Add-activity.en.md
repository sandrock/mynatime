
[Index](AppCLI.0Index.en.md) - [Back: Authenticate with service](AppCLI.Authenticate.en.md) - Next: nothing here yet

---

Add an activity - Mynatime CLI app documentation
====================================

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

---

[Index](AppCLI.0Index.en.md) - [Back: Authenticate with service](AppCLI.Authenticate.en.md) - Next: nothing here yet
