param(
    [string]$Version = "latest",
    [string]$InstallDir = "$env:LOCALAPPDATA\TermBullet\bin",
    [switch]$NoPath
)

$ErrorActionPreference = "Stop"

$Repository = "AlexsanderCallou/TermBullet"
$AssetPattern = "windows_x64.zip"

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message"
}

function Get-Release {
    if ($Version -eq "latest") {
        return Invoke-RestMethod -Uri "https://api.github.com/repos/$Repository/releases/latest"
    }

    return Invoke-RestMethod -Uri "https://api.github.com/repos/$Repository/releases/tags/$Version"
}

function Add-ToUserPath {
    param([string]$Directory)

    $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $entries = @()
    if (-not [string]::IsNullOrWhiteSpace($currentPath)) {
        $entries = $currentPath -split ";"
    }

    if ($entries -contains $Directory) {
        return
    }

    $newPath = if ([string]::IsNullOrWhiteSpace($currentPath)) {
        $Directory
    } else {
        "$currentPath;$Directory"
    }

    [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
    $env:Path = "$env:Path;$Directory"
}

$isWindowsRuntime = if (Get-Variable -Name IsWindows -ErrorAction SilentlyContinue) {
    $IsWindows
} else {
    $env:OS -eq "Windows_NT"
}

if (-not $isWindowsRuntime) {
    throw "This installer currently supports Windows only."
}

if (-not [Environment]::Is64BitOperatingSystem) {
    throw "This installer currently supports Windows x64 only."
}

Write-Step "Resolving TermBullet release ($Version)"
$release = Get-Release

$archiveAsset = $release.assets | Where-Object { $_.name -like "*$AssetPattern" } | Select-Object -First 1
if ($null -eq $archiveAsset) {
    throw "Could not find a Windows x64 asset in release $($release.tag_name)."
}

$checksumAsset = $release.assets | Where-Object { $_.name -like "*checksums.txt" } | Select-Object -First 1
if ($null -eq $checksumAsset) {
    throw "Could not find checksum asset in release $($release.tag_name)."
}

$tempRoot = Join-Path ([IO.Path]::GetTempPath()) "termbullet-install-$([Guid]::NewGuid().ToString('N'))"
$archivePath = Join-Path $tempRoot $archiveAsset.name
$checksumPath = Join-Path $tempRoot $checksumAsset.name
$extractPath = Join-Path $tempRoot "extract"

try {
    New-Item -ItemType Directory -Path $tempRoot, $extractPath -Force | Out-Null

    Write-Step "Downloading $($archiveAsset.name)"
    Invoke-WebRequest -Uri $archiveAsset.browser_download_url -OutFile $archivePath

    Write-Step "Downloading $($checksumAsset.name)"
    Invoke-WebRequest -Uri $checksumAsset.browser_download_url -OutFile $checksumPath

    Write-Step "Verifying SHA256 checksum"
    $expectedHash = (Get-Content $checksumPath | Where-Object { $_ -like "*$($archiveAsset.name)" } | Select-Object -First 1).Split(" ")[0]
    if ([string]::IsNullOrWhiteSpace($expectedHash)) {
        throw "Could not find checksum entry for $($archiveAsset.name)."
    }

    $actualHash = (Get-FileHash $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -ne $expectedHash.ToLowerInvariant()) {
        throw "Checksum mismatch. Expected $expectedHash but got $actualHash."
    }

    Write-Step "Extracting archive"
    Expand-Archive -Path $archivePath -DestinationPath $extractPath -Force

    $executable = Get-ChildItem -Path $extractPath -Filter "termbullet.exe" -Recurse | Select-Object -First 1
    if ($null -eq $executable) {
        throw "Archive does not contain termbullet.exe."
    }

    Write-Step "Installing to $InstallDir"
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    Copy-Item -Path $executable.FullName -Destination (Join-Path $InstallDir "termbullet.exe") -Force

    if (-not $NoPath) {
        Write-Step "Adding install directory to user PATH"
        Add-ToUserPath -Directory $InstallDir
    }

    Write-Step "Installation complete"
    Write-Host ""
    Write-Host "Run: termbullet --help"
    Write-Host "If this is a new PATH entry, open a new terminal before running TermBullet."
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item -Path $tempRoot -Recurse -Force
    }
}
