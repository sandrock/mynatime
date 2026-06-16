
Releasing
=========

This document describes how to publish a new release of Mynatime.

## Prerequisites

- Push access to the repository
- The `master` branch must be green (CI passing)

## Versioning

Versions follow [Semantic Versioning](https://semver.org/) with a `v` prefix: `v0.3.4`, `v1.0.0`, etc.
Pre-releases use the `-beta.N` suffix: `v0.4.0-beta.1`.

[MinVer](https://github.com/adamralph/minver) derives the version automatically from the git tag at
build time. The tag must be on its own commit — do not reuse a commit that already carries a
release tag, or MinVer may compute the wrong version.

## Steps

### 1. Prepare the commit

Make sure `master` contains everything that should go into the release.
All tests must pass and the build must be clean.

### 2. Create and push the tag

Tag the tip of `master` with the new version:

```bash
git tag vX.Y.Z
git push origin vX.Y.Z
```

> **Container users:** `origin` uses SSH and is unreachable inside the container.
> Push via the HTTPS remote instead: `git push claude vX.Y.Z`

The tag must be on a commit that is not already tagged with another release version.

### 3. Publish the GitHub release

Go to **Releases → Draft a new release** on GitHub and:

- Select the tag you just pushed
- Set the release title to the version (e.g. `v0.3.5`)
- Write a changelog in the description (features, fixes, breaking changes)
- Check **Set as pre-release** if this is a beta
- Click **Publish release**

Publishing the release triggers the CI release workflow automatically.

### 4. Verify the artifacts

The workflow builds two assets and uploads them to the release:

| Asset | Target |
|---|---|
| `mynatime-X.Y.Z-linux-x64.tar.gz` | Linux x64 |
| `mynatime-X.Y.Z-win-x64.zip` | Windows x64 |

Once the workflow finishes, download one of the assets and run `mynatime --version` to confirm it
reports the expected version (e.g. `0.3.5`, not an alpha/commit-hash string).

### 5. Announce

Update any relevant documentation or notify users as appropriate.

## Troubleshooting

**Version shows `0.0.0-alpha.0.N+<hash>` instead of the release version**

The CI workflow passes `MinVerVersionOverride` derived from the release tag name, so this should
not happen as long as the release tag name is a valid semver (e.g. `v0.3.6`).

If it does occur, the most likely cause is that the tag name on the GitHub release is malformed.
Delete and re-create the release with a correctly named tag to re-trigger the workflow.
