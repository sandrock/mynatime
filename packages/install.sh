#!/usr/bin/env bash
set -euo pipefail

REPO="sandrock/mynatime"
APP_NAME="mynatime"
DOTNET_VERSION="8"

# ── helpers ──────────────────────────────────────────────────────────────────

die()  { echo "ERROR: $*" >&2; exit 1; }
info() { echo "  $*"; }

# Run a command with sudo only when not already root
elevate() {
    if [ "$EUID" -eq 0 ]; then
        "$@"
    else
        sudo "$@"
    fi
}

# ── mode detection ────────────────────────────────────────────────────────────

MODE=""
YES=false
PRERELEASE=false
for arg in "$@"; do
    case "$arg" in
        --system)      MODE=system ;;
        --user)        MODE=user   ;;
        --yes | -y)    YES=true    ;;
        --prerelease)  PRERELEASE=true ;;
        --help | -h)
            echo "Usage: install.sh [OPTIONS]"
            echo ""
            echo "Install or update the Mynatime CLI app."
            echo ""
            echo "Options:"
            echo "  --system      Install system-wide to /usr/local/ (requires sudo)"
            echo "  --user        Install for current user to ~/.local/"
            echo "  --prerelease  Install the latest pre-release instead of stable"
            echo "  --yes, -y     Skip confirmation prompt (for scripted use)"
            echo "  --help, -h    Show this help"
            echo ""
            echo "Without --system or --user, the mode is auto-detected:"
            echo "  sudo available → system install, otherwise → user install."
            echo ""
            echo "After install, use 'mynatime-update' to update."
            exit 0
            ;;
    esac
done

if [ -z "$MODE" ]; then
    if sudo -n true 2>/dev/null; then
        MODE=system
    else
        MODE=user
    fi
fi

# ── paths ─────────────────────────────────────────────────────────────────────

if [ "$MODE" = system ]; then
    LIB_DIR="/usr/local/lib/$APP_NAME"
    BIN_DIR="/usr/local/bin"
else
    LIB_DIR="$HOME/.local/lib/$APP_NAME"
    BIN_DIR="$HOME/.local/bin"
fi

DESKTOP_DIR="$HOME/.local/share/applications"

# ── check for existing install ────────────────────────────────────────────────

EXISTING_VERSION=""
if [ -x "$LIB_DIR/$APP_NAME" ]; then
    EXISTING_VERSION=$("$LIB_DIR/$APP_NAME" --version 2>/dev/null || true)
fi

# ── check .NET ────────────────────────────────────────────────────────────────

has_dotnet() {
    dotnet --list-runtimes 2>/dev/null | grep -q "Microsoft.NETCore.App ${DOTNET_VERSION}\."
}

NEED_DOTNET=false
has_dotnet || NEED_DOTNET=true

# ── fetch latest release info ─────────────────────────────────────────────────

if $PRERELEASE; then
    # most recent release of any kind
    RELEASE_JSON=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases" \
        | grep -A5 '"tag_name"' | head -20)
else
    # most recent stable release only
    RELEASE_JSON=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest")
fi

RELEASE_TAG=$(echo "$RELEASE_JSON" | grep '"tag_name"' | head -1 | cut -d '"' -f4)
RELEASE_URL=$(echo "$RELEASE_JSON" | grep 'browser_download_url' | grep 'linux-x64.tar.gz' | head -1 | cut -d '"' -f4)

[ -z "$RELEASE_TAG" ] && die "Could not determine latest release tag from GitHub."
[ -z "$RELEASE_URL" ] && die "Could not find a linux-x64 tarball in release $RELEASE_TAG."

# ── summary + confirmation ────────────────────────────────────────────────────

echo ""
echo "Mynatime installer"
echo ""
if [ -n "$EXISTING_VERSION" ]; then
    echo "  Update:  $EXISTING_VERSION  →  $RELEASE_TAG"
else
    echo "  Version: $RELEASE_TAG"
fi
$PRERELEASE && echo "  Channel: pre-release"
if [ "$MODE" = system ]; then
    echo "  Mode:    system  (requires sudo)"
else
    echo "  Mode:    user"
fi
echo "  Install: $LIB_DIR/"
echo "  Commands: m, mynatime, mynatime-update"
if $NEED_DOTNET; then
    echo ""
    echo "  .NET ${DOTNET_VERSION} runtime not found — will be installed."
fi
echo ""

if ! $YES; then
    read -r -p "Continue? [Y/n] " REPLY
    case "$REPLY" in
        [nN]*) echo "Aborted."; exit 0 ;;
    esac
fi

# ── step 1: .NET runtime ──────────────────────────────────────────────────────

if $NEED_DOTNET; then
    echo ""
    echo "Installing .NET ${DOTNET_VERSION} runtime..."

    if [ "$MODE" = system ]; then
        installed=false

        if command -v apt-get &>/dev/null; then
            if ! apt-cache show "dotnet-runtime-${DOTNET_VERSION}.0" &>/dev/null 2>&1; then
                info "Adding Microsoft package feed..."
                DISTRO_ID=$(. /etc/os-release && echo "$ID")
                DISTRO_VERSION=$(. /etc/os-release && echo "$VERSION_ID")
                PKG="packages-microsoft-prod.deb"
                wget -q "https://packages.microsoft.com/config/${DISTRO_ID}/${DISTRO_VERSION}/${PKG}" -O "/tmp/${PKG}"
                elevate dpkg -i "/tmp/${PKG}"
                elevate apt-get update -q
            fi
            elevate apt-get install -y "dotnet-runtime-${DOTNET_VERSION}.0" && installed=true
        fi

        if ! $installed && command -v dnf &>/dev/null; then
            elevate dnf install -y "dotnet-runtime-${DOTNET_VERSION}.0" && installed=true
        fi

        if ! $installed; then
            info "Package manager install failed. Falling back to dotnet-install.sh..."
            DOTNET_INSTALL_DIR="/usr/local/share/dotnet"
            curl -fsSL https://dot.net/v1/dotnet-install.sh | \
                elevate bash -s -- --runtime dotnet --channel "${DOTNET_VERSION}.0" --install-dir "$DOTNET_INSTALL_DIR"
            if ! grep -q "$DOTNET_INSTALL_DIR" /etc/environment 2>/dev/null; then
                elevate bash -c "echo 'PATH=\"$DOTNET_INSTALL_DIR:\$PATH\"' >> /etc/environment"
                export PATH="$DOTNET_INSTALL_DIR:$PATH"
            fi
        fi
    else
        curl -fsSL https://dot.net/v1/dotnet-install.sh | \
            bash -s -- --runtime dotnet --channel "${DOTNET_VERSION}.0" --install-dir "$HOME/.dotnet"
        for RC in "$HOME/.bashrc" "$HOME/.zshrc"; do
            if [ -f "$RC" ] && ! grep -q '\.dotnet' "$RC"; then
                echo 'export PATH="$HOME/.dotnet:$PATH"' >> "$RC"
                info "Added ~/.dotnet to PATH in $RC"
            fi
        done
        export PATH="$HOME/.dotnet:$PATH"
    fi
fi

# ── step 2: download tarball ──────────────────────────────────────────────────

echo ""
echo "Downloading $RELEASE_TAG..."

TARBALL="/tmp/${APP_NAME}-latest.tar.gz"
wget -q --show-progress -O "$TARBALL" "$RELEASE_URL"

# ── step 3: install / update ──────────────────────────────────────────────────

echo ""
echo "Installing to $LIB_DIR..."

if [ "$MODE" = system ]; then
    elevate rm -rf "$LIB_DIR"
    elevate mkdir -p "$LIB_DIR" "$BIN_DIR"
    elevate tar -xzf "$TARBALL" -C "$LIB_DIR" --strip-components=1

    elevate ln -sf "$LIB_DIR/$APP_NAME" "$BIN_DIR/$APP_NAME"
    elevate ln -sf "$LIB_DIR/$APP_NAME" "$BIN_DIR/m"
    UPDATE_ARGS="--system --yes"
    $PRERELEASE && UPDATE_ARGS="$UPDATE_ARGS --prerelease"
    elevate tee "$BIN_DIR/${APP_NAME}-update" > /dev/null << EOF
#!/usr/bin/env bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/master/packages/install.sh | sudo bash -s -- $UPDATE_ARGS
EOF
    elevate chmod +x "$BIN_DIR/${APP_NAME}-update"
else
    rm -rf "$LIB_DIR"
    mkdir -p "$LIB_DIR" "$BIN_DIR"
    tar -xzf "$TARBALL" -C "$LIB_DIR" --strip-components=1

    ln -sf "$LIB_DIR/$APP_NAME" "$BIN_DIR/$APP_NAME"
    ln -sf "$LIB_DIR/$APP_NAME" "$BIN_DIR/m"
    UPDATE_ARGS="--user --yes"
    $PRERELEASE && UPDATE_ARGS="$UPDATE_ARGS --prerelease"
    cat > "$BIN_DIR/${APP_NAME}-update" << EOF
#!/usr/bin/env bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/master/packages/install.sh | bash -s -- $UPDATE_ARGS
EOF
    chmod +x "$BIN_DIR/${APP_NAME}-update"

    for RC in "$HOME/.bashrc" "$HOME/.zshrc"; do
        if [ -f "$RC" ] && ! grep -q '\.local/bin' "$RC"; then
            echo 'export PATH="$HOME/.local/bin:$PATH"' >> "$RC"
            info "Added ~/.local/bin to PATH in $RC — run: source $RC"
        fi
    done
fi

rm -f "$TARBALL"

# ── step 4: desktop entry ─────────────────────────────────────────────────────

mkdir -p "$DESKTOP_DIR"
cat > "$DESKTOP_DIR/${APP_NAME}.desktop" << EOF
[Desktop Entry]
Name=Mynatime CLI
Exec=$APP_NAME
Icon=utilities-terminal
Type=Application
Categories=Utility;
Terminal=true
Comment=Command-line client for time tracking
EOF

# ── done ─────────────────────────────────────────────────────────────────────

echo ""
echo "Done. Run: m"
