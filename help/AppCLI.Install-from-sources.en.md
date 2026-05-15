
[Back: Index](AppCLI.0Index.en.md) - [Next: Mynatime commands](AppCLI.Commands-overview.en.md)

---

Installing the Mynatime CLI app
====================================

NOTE: there are pending work tasks to publish the app on [winget](https://github.com/sandrock/mynatime/issues/8) and [flatpak](https://github.com/sandrock/mynatime/issues/7)


Install from pre-built binary (recommended)
-----------------------------

The easiest way. Works on GNU/Linux x86-64. The script installs .NET if needed.

**With sudo** (installs system-wide to `/usr/local/`):
```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/master/packages/install.sh | sudo bash
```

**Without sudo** (installs to `~/.local/`):
```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/master/packages/install.sh | bash
```

Both commands create two aliases: `mynatime` and `m`.

Verify the install:
```bash
m help
```


Update from pre-built binary
-----------------------------

Please keep the app up-to-date because the service can change its API anytime.

```bash
mynatime-update
```


Install from sources
-----------------------------

Use this if you want to work on the code or if no pre-built binary is available for your platform.

**Requirements** — install from your package manager:

- dotnet-sdk (>=8)
- dotnet-runtime (>=8)

The commands below are for GNU/Linux. Windows users: use *git bash*, WSL, or PowerShell.

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
alias "m=~/.opt/mynatime/src/Mynatime/bin/Release/net8.0/Mynatime.CLI"
m help
```
4. Add the alias to your `~/.bashrc` / `~/.zshrc` to make it permanent


Update from sources
-----------------------------

```bash
cd ~/.opt/mynatime
git pull
dotnet build src/Mynatime.sln -c Release -v q
```

---

[Back: Index](AppCLI.0Index.en.md) - [Next: Mynatime commands](AppCLI.Commands-overview.en.md)
