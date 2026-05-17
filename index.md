---
title: Home
nav_order: 1
---

# Mynatime

Unofficial CLI app for Manatime time tracking. Offline-first, git-style workflow: stage activities locally, review them, commit when online.

> Use at your own risk. This is not affiliated with Manatime.

## Install

**Linux** — one-liner, installs .NET if needed:

```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | bash
```

**Windows** — open PowerShell:

```powershell
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex
```

Both install `mynatime` and `m` commands and write a `mynatime-update` command for future updates.

## Quick start

```bash
# create a profile
m pro add

# pull activity categories from the service
m act cat refresh

# track your work
m act start myproject
m act stop

# review and publish
m status
m commit
```

## Why use this?

- Offline-first: prepare changes locally, push when online
- Git-style workflow: stage, review, commit
- Faster than the web UI for common operations
- Simple time tracking with start/stop events

---

[📖 Full documentation](help/AppCLI.0Index.en.md)
