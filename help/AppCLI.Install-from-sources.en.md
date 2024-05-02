
[Back: Index](AppCLI.0Index.en.md) - [Next: Mynatime commands](AppCLI.Commands-overview.en.md)

---

Installing from sources - Mynatime CLI app documentation
====================================

Get build tools
-----------------------------

Install these things from your package manager:

- dotnet-sdk (>=6)
- dotnet-runtime (>=6)

NOTE: there are pending work tasks to publish the app on [winget](https://github.com/sandrock/mynatime/issues/8) and [flatpak](https://github.com/sandrock/mynatime/issues/7)


Notes on operating system
-----------------------------

The commands here are for GNU/Linux users. 

Windows users: use *git bash* or WSL or powershell or anything else.



Install from sources
-----------------------------

1. Clone the repository  
```bash
mkdir ~/.opt
git clone git@github.com:sandrock/mynatime.git ~/.opt/mynatime
cd ~/.opt/mynatime
```
2. Build it
```bash
dotnet build src/Mynatime.sln -c Release -v q
```
3. Alias the CLI app and verify
```bash
alias "m=~/.opt/mynatime/src/Mynatime/bin/Release/net6.0/Mynatime.CLI"
which m
m help
```
4. Add the alias to your `~/.bashrc` / `~/.zshrc`


Update from sources
-----------------------------

Please keep the app up-to-date because the service can change its API anytime. 

1. Update sources
```bash
cd ~/.opt/mynatime
git pull
```
2. Build it
```bash
dotnet build src/Mynatime.sln -c Release -v q
```

---

[Back: Index](AppCLI.0Index.en.md) - [Next: Mynatime commands](AppCLI.Commands-overview.en.md)
