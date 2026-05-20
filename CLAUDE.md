# CLAUDE.md

## About this repo

[Describe the repo here]

## .NET SDK

This solution targets **net8.0**. Before running any `dotnet` command, check whether the SDK is available:

```bash
which dotnet || echo "not found"
```

If not found and running in a container (no sudo), install via the official script and export env vars for the whole session:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir /home/claude/.dotnet
export DOTNET_ROOT=/home/claude/.dotnet
export PATH=$PATH:/home/claude/.dotnet
```

After that, plain `dotnet build`, `dotnet test`, etc. work without any prefix.

## Session initialization

At the start of every session, offer to:
1. Inspect the project and identify the tech stack
2. Install dependencies / restore packages
3. Verify the build
4. Run the test suite to establish a baseline

Ask for confirmation before running anything. Skip if the user jumps straight to a specific task.
