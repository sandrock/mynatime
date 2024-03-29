
[Index](AppCLI.0Index.en.md) - Back: [Authenticate with service](AppCLI.Authenticate.en.md) - Next: [Add an activity](AppCLI.Add-activity.en.md)

---

Tracking activity - Mynatime CLI app documentation
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


Tracking activity
-----------------------------------

### Workflow

The best way to add activity is to track your work as you do it. You will use the `activity start/stop` command to specify the activity you start or stop. 

Later, a list of activities will be generated from these declarations. You will be able to push these to the service. 


### Example 1

The basic usage is:

- at `08:51`: you arrive at work
    - `m act start`
- at `09:07`: later you start working on project1
    - `m act start project1`
- at `12:08`: and you stop
    - `m act stop`
- at `13:45`: you start working on something
    - `m act start`
- at `17:41`: when you stop, you indicate the project
    - `m act stop project1`
- at `17:41`: you know you will leave at 17:45
    - `m act stop 1745`
- at `17:41`: you can review the resulting activities
    - `m status`
- at `17:42`: and you can save them
    - `m commit`

The status command will display the resulting activities:

```
0	Activity tracker
Activities:
- 2022-10-30 08:51 09:07
- 2022-10-30 09:07 12:08 Project1
- 2022-10-30 13:45 17:41 Project1
- 2022-10-30 17:41 17:45
```

The status command is important to use because it will recognize usual input errors and display them. 


### Tracking warnings and errors

When you use the status command, these warnings may be displayed:

- *"Nightly activity between 2022-10-29 and 2022-10-30. " (NightlyItem)*
    - Either you forgot to `stop` when leaving work, either you worked past midnight.
- *"Multiple days activity between 2022-10-27 and 2022-10-29. (ManyDaysItem)"*
    - A generated activity would span across many days. This is unusual. 

And these errors may be displayed:

- *"Stop event 2022-10-27-1000 is not following a start event." (StopNotFollowingStart)*
- *"Event 2022-10-27-1100 is of unknown type." (UnknownEventType)*
    - May occur if you manually change the data file.


### Example 2

Here a sample showing you what happens when you forget to specify some of your activity change events.

Say you arrive at work, and start working on project1.

```bash
# time is 0840
m act start project1
```

You start working on project1 and it is time for the daily meeting.

```bash
# time is 0930
m act start daily
```

The daily meeting ended... You went to work on project2... It's noon, time for lunch...
You forgot to end the daily and set the current project. You can do it now, specifying the times. 

```bash
# time is 1202
m act stop 0940       # this will stop the "daily"
m act start 0940      # this will start something
m act stop project2   # and stop if, associating it to project2
```

Each time you switch to a different task, you just have to "start" it.  When you stop, you "stop".

At the end of the day, you can review the activities:

```bash
m act status         # display full details
m status             # display summary of changes
```

And publish it:

```bash
m commit
```


### Command usage

Here is all you can do with activity tracking:

```bash
m act start [time] [category]              # starts an activity
m act stop [time] [category]               # stops  an activity
m act status                               # lists current activities
m act clear                                # removes all events
m act start/stop [date] [time] [category]  # you can specify a date
                        [--message|-m <message>]    # you can add a comment
```



---

[Index](AppCLI.0Index.en.md) - Back: [Authenticate with service](AppCLI.Authenticate.en.md) - Next: [Add an activity](AppCLI.Add-activity.en.md)
