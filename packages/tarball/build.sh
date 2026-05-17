#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
DIST_DIR="$REPO_ROOT/dist"
PUBLISH_DIR="$DIST_DIR/publish-linux-x64"

cd "$REPO_ROOT"

# Read version from the nearest git tag
VERSION=$(git describe --tags --match "v*" --abbrev=0 2>/dev/null | sed 's/^v//' || true)
if [ -z "$VERSION" ]; then
    echo "ERROR: No git tag found. Tag the repo first (e.g. git tag v1.0.0)." >&2
    exit 1
fi

TARBALL="$DIST_DIR/mynatime-${VERSION}-linux-x64.tar.gz"

echo "Building mynatime $VERSION for linux-x64..."

dotnet publish src/Mynatime/Mynatime.CLI.csproj \
    -c Release \
    -r linux-x64 \
    --no-self-contained \
    -o "$PUBLISH_DIR"

mv "$PUBLISH_DIR/Mynatime.CLI" "$PUBLISH_DIR/mynatime"

mkdir -p "$DIST_DIR"
tar -czf "$TARBALL" \
    -C "$PUBLISH_DIR" \
    --transform 's,^,mynatime/,' \
    .

echo ""
echo "Done: $TARBALL"
echo "Size: $(du -sh "$TARBALL" | cut -f1)"
