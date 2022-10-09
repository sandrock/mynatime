
[Back: Index](AppCLI.0Index.en.md) - [Next: Mynatime commands](AppCLI.Commands-overview.en.md)

---

Installing from sources - Mynatime CLI app documentation
====================================

Get tools
-----------------------------

Install these things from your package manager:

- dotnet-sdk (>=6)
- dotnet-runtime (>=6)


Install from sources
-----------------------------

1. Clone the repository
2. Build it
```bash
cd src
dotnet build
```
3. Alias the CLI app and verify
```bash
alias "mynatime=/home/me/dd/github/sandrock/mynatime/src/Mynatime/bin/Debug/net6.0/Mynatime.CLI"
which mynatime
mynatime help
```
4. Add the alias to your `~/.bashrc`

Update from sources
-----------------------------

1. Update sources
```bash
git pull
```
2. Build it
```bash
cd src
dotnet build
```

---

[Back: Index](AppCLI.0Index.en.md) - [Next: Mynatime commands](AppCLI.Commands-overview.en.md)
