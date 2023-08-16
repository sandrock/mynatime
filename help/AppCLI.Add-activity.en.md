
[Index](AppCLI.0Index.en.md) - Back: [Tracking current activity](AppCLI.Tracking-activity.en.md) - Next: [Tips and tricks](AppCLI.Tips-tricks.en.md)

---

Add an activity - Mynatime CLI app documentation
====================================

Refresh the activity categories
-----------------------------------

First, you have to pull the list of activity categories from the service.

```bash
m activity category refresh
m act cat refresh
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
m activity category
m act cat
```

If a category is created by your manager, you will need to execute the refresh command again.

You can search a category:

```bash
m activity category search project something
m act cat search project something
```


Adding past activity
-----------------------------------

### Command usage

Activity tracking uses a list of events to create a list of activities. You can skip tracking and create activities. This is useful when you have create past activities. 

```bash
m  act add [date] <duration> [category]                         # adds an activity
m  act add <date> <time-start> [date-end] <time-end> [category] # adds an activity
                        [--message|-m <message>]                       # you can add a comment
```


### Example 1

Now let's add an activity for the current day, using start time (08:30) and end time (11:50) for project "Internal".

```bash
m act add 0830 1150 Internal
```

Now let's add an activity for yesterday (2022-09-15), using start time (08:30) and end time (11:50) for project "Internal".

```bash
m act add 2022-09-15 0830 1150 Internal
```

You can also use duration (2.7 hours) instead of times (for "Project 123").

```bash
m act add 2022-09-15 2.7 Project-123
```

You can also set a comment using the `-m` argument:

```bash
m act add 2022-09-15 2.7 Project-123 -m "help John on the design"
```

All this is only saved on your computer. You can review what you created:

```bash
m status
```

If it looks okay, you can publish to the service:

```bash
m commit
```


---

[Index](AppCLI.0Index.en.md) - Back: [Tracking current activity](AppCLI.Tracking-activity.en.md) - Next: [Tips and tricks](AppCLI.Tips-tricks.en.md)
