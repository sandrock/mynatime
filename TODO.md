
TODO list for Mynatime
======================

Goals
------------------

1. Fast way to add an activity item (CLI)
2. Start/stop current activity item, be alerted when no current activity (GUI)


Client
------------------

- [x] Authenticate
- [ ] Obtain list of available activity categories
- [ ] Create a activity item


CLI
------------------

- [ ] Authenticate, creating a profile file
- [ ] Use existing profile file
- [ ] Goal 1


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

