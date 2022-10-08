
TODO list for Mynatime
======================

Goals
------------------

1. Fast way to add an activity item (CLI)
2. Start/stop current activity item, be alerted when no current activity (GUI)


Client
------------------

- [x] Authenticate
- [x] Obtain list of available activity categories
- [ ] Create a activity item


CLI
------------------

- [x] Authenticate, creating a profile file
- [x] Use existing profile file
- [x] Activity categories
  - [x] List categories
  - [x] Refresh categories from source
  - [x] Search categories
  - [ ] Create alias for a category (when best-match-search does not work)
- [ ] Goal 1
  - [ ] Add activity items
  - [ ] Status
  - [ ] Commit
- [ ] Goal 2
  - [ ] Track activity
  - [ ] Status
  - [ ] Commit
- [ ] Code
  - [ ] Have a proper way for a command to display errors and stop
  - [x] Have a proper way for a command to display usage
  - [ ] The way arguments are parsed is questionable 
  - [ ] `bool Command.ParseArgs()` should return true when args match the command, even when args are invalid

### Authenticate

Authenticate:

```bash
mynatime +profile [email]
```

Verify:

```bash
mynatime profiles
mynatime profile check
```


### Activity categories

```bash
mynatime activity categories
```


### Add activity items

```bash
mynatime +act category123 2.5     # today   I did 2h 30min of Category123
mynatime +act 2022-04-16 4 cat5   # at date I did 4h       of cat5
mynatime +act 0855 1215 internal  # today, from 0855 to 1215, I was on internal
mynatime status                   # review   pending changes
mynatime commit                   # save all pending changes
```

The "activity add" command can be written as "+activity" or "+act".


### Track activity

```bash
mynatime activity start internal   # at 0834
mynatime activity start project4   # at 1054
mynatime activity stop             # at 1223
mynatime act start 1400 project4   # at 1432
mynatime act stop 1730             # at 1735
mynatime status                    # review   pending changes
mynatime commit                    # save all pending changes
```


GUI
------------------

- [x] Authenticate, creating a profile file
- [x] Use existing profile file
- [ ] Goal 2


Extras
------------------

- Decent logging
  - [avalonia logging](https://docs.avaloniaui.net/docs/getting-started/logging-errors-and-warnings) does not work
  - how to use [dotnet core logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line) without DI?
  - will use a Logger class to centralize logging calls
    - maybe [this](https://github.com/zkSNACKs/WalletWasabi/blob/3b56845466b6d228585d879c18ca3dc79e2e80dd/WalletWasabi/Logging/Logger.cs) is correct?
  - how to do it properly?

