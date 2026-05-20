# CLAUDE.md

## About this repo

[Describe the repo here]

## Tooling setup (container environment)

This project runs inside a Docker container with no sudo access. Tools are installed to `/home/claude/.dotnet` and do not persist between sessions. At the start of each session, export PATH once:

```bash
export DOTNET_ROOT=/home/claude/.dotnet
export PATH=$PATH:/home/claude/.dotnet
```

### .NET SDK 8.0

This solution targets **net8.0**. Check availability, install if missing:

```bash
which dotnet || curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir /home/claude/.dotnet
```

### gh CLI

Check availability, install if missing:

```bash
which gh || (curl -fsSL https://github.com/cli/cli/releases/download/v2.72.0/gh_2.72.0_linux_amd64.tar.gz -o /tmp/gh.tar.gz && tar -xz -C /tmp -f /tmp/gh.tar.gz && cp /tmp/gh_2.72.0_linux_amd64/bin/gh /home/claude/.dotnet/gh && chmod +x /home/claude/.dotnet/gh)
```

Then authenticate if needed:

```bash
gh auth status || gh auth login
```

## Session initialization

At the start of every session, offer to:
1. Inspect the project and identify the tech stack
2. Install dependencies / restore packages
3. Verify the build
4. Run the test suite to establish a baseline

Ask for confirmation before running anything. Skip if the user jumps straight to a specific task.
