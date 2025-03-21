# Define variables
$baseServerPath = Join-Path $PSScriptRoot "BaseServer"
$serverPath = Join-Path $PSScriptRoot "Server"
$pluginsFolder = Join-Path $serverPath "plugins"
$openMpZip = "open.mp-win-x86.zip"
$sampSharpZip = "SampSharp.zip"
$streamerZip = "Streamer.zip"
$sampSharpRepo = "ikkentim/SampSharp"
$streamerRepo = "samp-incognito/samp-streamer-plugin"
$colAndreasRepo = "Pottus/ColAndreas"
$dotnetFolder = Join-Path $serverPath "dotnet"
$dotnetUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/6.0.428/dotnet-sdk-6.0.428-win-x86.zip"
$dotnetZip = "dotnet-runtime.zip"
$colAndreasWizardExe = "ColAndreasWizard.exe"

# Ensure required directories exist
if (!(Test-Path -Path $serverPath)) {
    New-Item -ItemType Directory -Path $serverPath | Out-Null
}
if (!(Test-Path -Path $pluginsFolder)) {
    New-Item -ItemType Directory -Path $pluginsFolder | Out-Null
}

# --- Download and Extract open.mp ---
$url = (Invoke-RestMethod -Uri "https://api.github.com/repos/openmultiplayer/open.mp/releases/latest").assets |
    Where-Object { $_.name -like $openMpZip } |
    Select-Object -ExpandProperty browser_download_url

Invoke-WebRequest -Uri $url -OutFile $openMpZip
Expand-Archive -Path $openMpZip -DestinationPath "./" -Force
Remove-Item -Path $openMpZip -Force

Write-Host "open.mp download and extraction complete!"

# --- Download and Extract SampSharp ---
$sampSharpUrl = (Invoke-RestMethod -Uri "https://api.github.com/repos/$sampSharpRepo/releases/latest").assets |
    Where-Object { $_.name -like "*.zip" } |
    Select-Object -ExpandProperty browser_download_url

Invoke-WebRequest -Uri $sampSharpUrl -OutFile $sampSharpZip
Expand-Archive -Path $sampSharpZip -DestinationPath "./SampSharpExtract" -Force
Remove-Item -Path $sampSharpZip -Force

Write-Host "SampSharp download and extraction complete!"

# Locate 'plugins' Folder and Move It
$sampSharpExtractedFolder = Get-ChildItem -Path "./SampSharpExtract" -Directory | Select-Object -First 1
if ($sampSharpExtractedFolder) {
    $sampSharpPlugins = Join-Path $sampSharpExtractedFolder.FullName "plugins"

    if (Test-Path -Path $sampSharpPlugins) {
        Move-Item -Path (Join-Path $sampSharpPlugins "*") -Destination $pluginsFolder -Force
        Write-Host "SampSharp plugins moved to Server/plugins!"
    } else {
        Write-Host "SampSharp plugins folder not found!"
    }
} else {
    Write-Host "Could not locate extracted SampSharp folder!"
}

# Cleanup SampSharp extracted files
Remove-Item -Path "./SampSharpExtract" -Recurse -Force

# --- Download and Extract Streamer Plugin ---
$streamerRelease = Invoke-RestMethod -Uri "https://api.github.com/repos/$streamerRepo/releases/latest"
$streamerAsset = $streamerRelease.assets | Where-Object { $_.name -match "samp-streamer-plugin-.*\.zip" } | Select-Object -First 1

if ($streamerAsset) {
    $streamerUrl = $streamerAsset.browser_download_url
    Invoke-WebRequest -Uri $streamerUrl -OutFile $streamerZip

    # Create a temporary extraction directory
    $tempExtractPath = "./StreamerExtract"
    if (Test-Path -Path $tempExtractPath) {
        Remove-Item -Path $tempExtractPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempExtractPath | Out-Null

    # Extract ZIP contents
    Expand-Archive -Path $streamerZip -DestinationPath $tempExtractPath -Force
    Remove-Item -Path $streamerZip -Force

    Write-Host "Streamer Plugin download and extraction complete!"

    # Move 'plugins' folder contents
    $streamerPlugins = Join-Path $tempExtractPath "plugins"
    if (Test-Path -Path $streamerPlugins) {
        Move-Item -Path (Join-Path $streamerPlugins "*") -Destination $pluginsFolder -Force
        Write-Host "Streamer Plugin moved to Server/plugins!"
    } else {
        Write-Host "Streamer Plugin's plugins folder not found!"
    }

    # Cleanup extracted files
    Remove-Item -Path $tempExtractPath -Recurse -Force
} else {
    Write-Host "Could not find a valid Streamer Plugin release!"
}

# --- Download and Place ColAndreas.dll in Plugins Folder ---
$colAndreasRelease = Invoke-RestMethod -Uri "https://api.github.com/repos/$colAndreasRepo/releases/latest"
$colAndreasAsset = $colAndreasRelease.assets | Where-Object { $_.name -match "ColAndreas.dll" } | Select-Object -First 1

if ($colAndreasAsset) {
    $colAndreasUrl = $colAndreasAsset.browser_download_url
    $colAndreasDll = Join-Path $pluginsFolder "ColAndreas.dll"

    # Download file (Fixed: Removed -Force, Added -UseBasicParsing)
    Invoke-WebRequest -Uri $colAndreasUrl -OutFile $colAndreasDll -UseBasicParsing

    Write-Host "ColAndreas.dll downloaded and placed in Server/plugins!"
} else {
    Write-Host "Could not find a valid ColAndreas.dll release!"
}

# --- Download ColAndreasWizard.exe and Start It ---
$colAndreasWizardAsset = $colAndreasRelease.assets | Where-Object { $_.name -match "ColAndreasWizard.exe" } | Select-Object -First 1

if ($colAndreasWizardAsset) {
    $colAndreasWizardUrl = $colAndreasWizardAsset.browser_download_url
    $colAndreasWizardPath = Join-Path $PSScriptRoot "ColAndreasWizard.exe"

    # Download without using -Force (fixed)
    Invoke-WebRequest -Uri $colAndreasWizardUrl -OutFile $colAndreasWizardPath -UseBasicParsing

    Write-Host "ColAndreasWizard.exe downloaded to root folder."

    # Ensure file exists before starting
    if (Test-Path -Path $colAndreasWizardPath) {
        Start-Process -FilePath $colAndreasWizardPath
        Write-Host "ColAndreasWizard.exe started."
    } else {
        Write-Host "Error: ColAndreasWizard.exe was not downloaded successfully!"
    }
} else {
    Write-Host "Could not find a valid ColAndreasWizard.exe release!"
}

# --- Download and Extract .NET 6.0 Runtime ---
if (!(Test-Path -Path $dotnetFolder)) {
    New-Item -ItemType Directory -Path $dotnetFolder | Out-Null
}

Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetZip
Expand-Archive -Path $dotnetZip -DestinationPath $dotnetFolder -Force
Remove-Item -Path $dotnetZip -Force

Write-Host ".NET 6.0 runtime downloaded and placed in Server/dotnet successfully!"

# --- Copy contents of BaseServer to Server, overwriting existing files ---
if (Test-Path -Path $baseServerPath) {
    Get-ChildItem -Path $baseServerPath | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $serverPath -Recurse -Force
    }
    Write-Host "BaseServer contents copied to Server successfully!"
} else {
    Write-Host "BaseServer folder not found!"
}

Write-Host "All tasks completed successfully!"