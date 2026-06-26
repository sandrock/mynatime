# CLAUDE.md

## About this repo

Mynatime — an unofficial, offline-first CLI (and experimental GUI) for the
Manatime time-tracking service. Stage / review / commit activity changes
locally, git-style, then sync when online. .NET 8, GPLv3. CLI entry point:
`src/Mynatime` (`Mynatime.CLI`); solution at `src/Mynatime.sln`.

## Tooling setup (container environment)

Runs in a container with no sudo. Tool directories are already on `PATH`
(including `$HOME/.dotnet`) — **never export PATH manually.** A "command not
found" means the tool isn't installed yet, not that PATH is wrong: install it
into a directory already on PATH (`$HOME/.dotnet`) and call it directly. Tools
installed this way do not persist between sessions.

### .NET SDK 8.0

Targets **net8.0**. Install if missing:

```bash
which dotnet || curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir "$HOME/.dotnet"
```

Build / test (filter build output to errors only):

```bash
dotnet build src/Mynatime.sln -c Debug -v q 2>&1 | grep ": error"
dotnet test  src/MynatimeCLI.Tests/Mynatime.CLI.Tests.csproj
```

### gh CLI

Used for releases (see RELEASING.md). Install if missing:

```bash
which gh || (
  v=$(curl -fsSL https://api.github.com/repos/cli/cli/releases/latest | sed -n 's/.*"tag_name": *"v\([^"]*\)".*/\1/p') &&
  curl -fsSL "https://github.com/cli/cli/releases/download/v${v}/gh_${v}_linux_amd64.tar.gz" -o /tmp/gh.tar.gz &&
  tar -xz -C /tmp -f /tmp/gh.tar.gz &&
  cp "/tmp/gh_${v}_linux_amd64/bin/gh" "$HOME/.dotnet/gh" && chmod +x "$HOME/.dotnet/gh"
)
```

Then authenticate if needed (the gh config dir must be owned by the session user):

```bash
gh auth status || gh auth login
```

