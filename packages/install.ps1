#Requires -Version 5.1
<#
.SYNOPSIS
    Install or update the Mynatime CLI app on Windows.
.DESCRIPTION
    Downloads the latest release from GitHub and installs it.
    Checks for the .NET runtime and installs it if missing.
.PARAMETER System
    Install system-wide to C:\Program Files\ (requires administrator).
.PARAMETER User
    Install for the current user to %LOCALAPPDATA%\.
.PARAMETER Prerelease
    Install the latest pre-release instead of stable.
.PARAMETER Yes
    Skip confirmation prompt (for scripted use).
.EXAMPLE
    irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex
.EXAMPLE
    .\install.ps1 -User -Yes
#>
[CmdletBinding()]
param(
    [switch]$System,
    [switch]$User,
    [switch]$Prerelease,
    [switch]$Yes
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Repo          = 'sandrock/mynatime'
$AppName       = 'mynatime'
$DotNetVersion = '8'

# ── helpers ──────────────────────────────────────────────────────────────────

function Die([string]$msg) { Write-Host "ERROR: $msg" -ForegroundColor Red; exit 1 }
function Info([string]$msg) { Write-Host "  $msg" }

# ── mode detection ────────────────────────────────────────────────────────────

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $System -and -not $User) {
    if ($isAdmin) { $System = $true } else { $User = $true }
}

if ($System -and -not $isAdmin) {
    Die "System install requires administrator privileges. Re-run as administrator, or use -User."
}

# ── paths ─────────────────────────────────────────────────────────────────────

if ($System) {
    $LibDir = "C:\Program Files\$AppName"
} else {
    $LibDir = "$env:LOCALAPPDATA\$AppName"
}

# ── check for existing install ────────────────────────────────────────────────

$ExistingVersion = ''
$ExePath = Join-Path $LibDir "$AppName.exe"
if (Test-Path $ExePath) {
    try { $ExistingVersion = (& $ExePath --version 2>$null) } catch {}
}

# ── check .NET ────────────────────────────────────────────────────────────────

function Test-DotNet {
    try {
        $runtimes = dotnet --list-runtimes 2>$null
        return [bool]($runtimes | Select-String "Microsoft.NETCore.App $DotNetVersion\.")
    } catch {
        return $false
    }
}

$NeedDotNet = -not (Test-DotNet)

# ── fetch latest release info ─────────────────────────────────────────────────

function Get-LatestAnyRelease {
    $list = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases" -ErrorAction SilentlyContinue
    if (-not $list -or $list.Count -eq 0) { return $null }
    $tag = $list[0].tag_name
    if (-not $tag) { return $null }
    return Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/tags/$tag" -ErrorAction SilentlyContinue
}

if ($Prerelease) {
    Info "Fetching latest release info (pre-releases included)..."
    $Release = Get-LatestAnyRelease
} else {
    Info "Fetching latest stable release info..."
    $Release = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/latest" -ErrorAction SilentlyContinue
    if (-not $Release -or -not $Release.tag_name) {
        Info "No stable release found. Falling back to latest pre-release."
        $Release = Get-LatestAnyRelease
        $Prerelease = $true
    }
}

if (-not $Release) { Die "Could not fetch release info from GitHub." }

$ReleaseTag = $Release.tag_name
$Asset      = $Release.assets | Where-Object { $_.name -like "*win-x64*.zip" } | Select-Object -First 1
$ReleaseUrl = if ($Asset) { $Asset.browser_download_url } else { $null }

Info "Tag: $ReleaseTag"
Info "URL: $ReleaseUrl"

if (-not $ReleaseTag) { Die "Could not determine latest release tag from GitHub." }
if (-not $ReleaseUrl) { Die "Could not find a win-x64 zip in release $ReleaseTag. The release workflow may not have run for this tag." }

# ── summary + confirmation ────────────────────────────────────────────────────

Write-Host ""
Write-Host "Mynatime installer"
Write-Host ""
if ($ExistingVersion) {
    Write-Host "  Update:  $ExistingVersion  ->  $ReleaseTag"
} else {
    Write-Host "  Version: $ReleaseTag"
}
if ($Prerelease) { Write-Host "  Channel: pre-release" }
if ($System)     { Write-Host "  Mode:    system" } else { Write-Host "  Mode:    user" }
Write-Host "  Install: $LibDir\"
Write-Host "  Commands: m, mynatime, mynatime-update"
if ($NeedDotNet) {
    Write-Host ""
    Write-Host "  .NET $DotNetVersion runtime not found — will be installed."
}
Write-Host ""

if (-not $Yes) {
    $reply = Read-Host "Continue? [Y/n]"
    if ($reply -match '^[nN]') { Write-Host "Aborted."; exit 0 }
}

# ── step 1: .NET runtime ──────────────────────────────────────────────────────

if ($NeedDotNet) {
    Write-Host ""
    Write-Host "Installing .NET $DotNetVersion runtime..."

    $installed = $false

    if ($System -and (Get-Command winget -ErrorAction SilentlyContinue)) {
        try {
            winget install --id "Microsoft.DotNet.Runtime.$DotNetVersion" --silent --accept-package-agreements --accept-source-agreements
            $installed = Test-DotNet
        } catch {}
    }

    if (-not $installed) {
        Info "Using dotnet-install.ps1..."
        $installScript = Join-Path $env:TEMP "dotnet-install.ps1"
        Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        $installDir = if ($System) { "C:\Program Files\dotnet" } else { "$env:LOCALAPPDATA\Microsoft\dotnet" }
        & $installScript -Runtime dotnet -Channel "$DotNetVersion.0" -InstallDir $installDir

        $scope = if ($System) { 'Machine' } else { 'User' }
        $currentPath = [Environment]::GetEnvironmentVariable('PATH', $scope)
        if ($currentPath -notlike "*$installDir*") {
            [Environment]::SetEnvironmentVariable('PATH', "$installDir;$currentPath", $scope)
            $env:PATH = "$installDir;$env:PATH"
            Info "Added $installDir to PATH."
        }
    }
}

# ── step 2: download zip ──────────────────────────────────────────────────────

Write-Host ""
Write-Host "Downloading $ReleaseTag..."

$ZipPath = Join-Path $env:TEMP "$AppName-latest.zip"
Invoke-WebRequest -Uri $ReleaseUrl -OutFile $ZipPath -UseBasicParsing

# ── step 3: install / update ──────────────────────────────────────────────────

Write-Host ""
Write-Host "Installing to $LibDir..."

if (Test-Path $LibDir) { Remove-Item $LibDir -Recurse -Force }
New-Item -ItemType Directory -Path $LibDir -Force | Out-Null
Expand-Archive -Path $ZipPath -DestinationPath $LibDir -Force
Remove-Item $ZipPath -Force

# ── step 4: mynatime-update.cmd ───────────────────────────────────────────────

$updateMode  = if ($System) { '-System' } else { '-User' }
$prerelFlag  = if ($Prerelease) { ' -Prerelease' } else { '' }
$updateUrl   = 'https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1'
$updateCmd   = "@echo off`r`nset `"f=%TEMP%\mynatime-install.ps1`"`r`n" `
             + "powershell -ExecutionPolicy Bypass -Command `"Invoke-WebRequest -Uri '$updateUrl' -OutFile '%f%' -UseBasicParsing`"`r`n" `
             + "powershell -ExecutionPolicy Bypass -File `"%f%`" $updateMode -Yes$prerelFlag`r`n" `
             + "del `"%f%`"`r`n"
Set-Content -Path (Join-Path $LibDir "mynatime-update.cmd") -Value $updateCmd -Encoding ASCII

# ── step 5: PATH ──────────────────────────────────────────────────────────────

$scope = if ($System) { 'Machine' } else { 'User' }
$currentPath = [Environment]::GetEnvironmentVariable('PATH', $scope)
if ($currentPath -notlike "*$LibDir*") {
    [Environment]::SetEnvironmentVariable('PATH', "$LibDir;$currentPath", $scope)
    $env:PATH = "$LibDir;$env:PATH"
    Info "Added $LibDir to PATH ($scope)."
}

# ── done ─────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Done. Run: m"
Write-Host "Note: restart your terminal for PATH changes to take effect."
