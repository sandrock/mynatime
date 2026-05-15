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
for arg in "$@"; do
    case "$arg" in
        --system) MODE=system ;;
        --user)   MODE=user   ;;
    esac
done

if [ -z "$MODE" ]; then
    if sudo -n true 2>/dev/null; then
        MODE=system
        info "Auto-detected: system install (sudo available)"
    else
        MODE=user
        info "Auto-detected: user install (no sudo)"
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

# ── step 1: .NET runtime ──────────────────────────────────────────────────────

echo ""
echo "Checking .NET runtime..."

has_dotnet() {
    dotnet --list-runtimes 2>/dev/null | grep -q "Microsoft.NETCore.App ${DOTNET_VERSION}\."
}

if has_dotnet; then
    info ".NET ${DOTNET_VERSION} runtime already installed."
elif [ "$MODE" = system ]; then
    info ".NET ${DOTNET_VERSION} runtime not found. Installing via package manager..."
    installed=false

    if command -v apt-get &>/dev/null; then
        # Add Microsoft feed if package is not known
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
        # Ensure it's on the system PATH
        if ! grep -q "$DOTNET_INSTALL_DIR" /etc/environment 2>/dev/null; then
            elevate bash -c "echo 'PATH=\"$DOTNET_INSTALL_DIR:\$PATH\"' >> /etc/environment"
            export PATH="$DOTNET_INSTALL_DIR:$PATH"
        fi
    fi
else
    # User mode: always use dotnet-install.sh to ~/.dotnet
    info ".NET ${DOTNET_VERSION} runtime not found. Installing to ~/.dotnet..."
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

# ── step 2: download latest tarball ──────────────────────────────────────────

echo ""
echo "Downloading latest release..."

RELEASE_URL=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" \
    | grep browser_download_url \
    | grep linux-x64.tar.gz \
    | cut -d '"' -f 4)

[ -z "$RELEASE_URL" ] && die "Could not find a linux-x64 tarball in the latest release."

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
    # Write update script
    elevate tee "$BIN_DIR/${APP_NAME}-update" > /dev/null << 'EOF'
#!/usr/bin/env bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/master/packages/install.sh | sudo bash -s -- --system
EOF
    elevate chmod +x "$BIN_DIR/${APP_NAME}-update"
else
    rm -rf "$LIB_DIR"
    mkdir -p "$LIB_DIR" "$BIN_DIR"
    tar -xzf "$TARBALL" -C "$LIB_DIR" --strip-components=1

    ln -sf "$LIB_DIR/$APP_NAME" "$BIN_DIR/$APP_NAME"
    ln -sf "$LIB_DIR/$APP_NAME" "$BIN_DIR/m"
    # Write update script
    cat > "$BIN_DIR/${APP_NAME}-update" << 'EOF'
#!/usr/bin/env bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/master/packages/install.sh | bash -s -- --user
EOF
    chmod +x "$BIN_DIR/${APP_NAME}-update"

    # Ensure ~/.local/bin is on PATH
    for RC in "$HOME/.bashrc" "$HOME/.zshrc"; do
        if [ -f "$RC" ] && ! grep -q '\.local/bin' "$RC"; then
            echo 'export PATH="$HOME/.local/bin:$PATH"' >> "$RC"
            info "Added ~/.local/bin to PATH in $RC"
            info "Run: source $RC  (or open a new terminal)"
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
