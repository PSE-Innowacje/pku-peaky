# Może wymagać uruchomienia polecenia w sesji PowerShell:
# Set-ExecutionPolicy RemoteSigned -Scope CurrentUser

#Requires -RunAsAdministrator

# 01.ps1 - Instalacja srodowiska deweloperskiego

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

$downloadDir = "$env:TEMP\pku-peaky-installers"
if (!(Test-Path $downloadDir)) { New-Item -ItemType Directory -Path $downloadDir | Out-Null }

Write-Host "Katalog pobierania: $downloadDir`n" -ForegroundColor Gray

# --- 1. .NET 10 SDK ---
Write-Host "=== Instalacja .NET 10 SDK ===" -ForegroundColor Cyan

$dotnetInstaller = "$downloadDir\dotnet-sdk-10.exe"
# Uzywamy oficjalnego skryptu instalacyjnego .NET
Write-Host "Pobieranie i instalacja .NET 10 SDK przez dotnet-install.ps1..."
$dotnetScript = "$downloadDir\dotnet-install.ps1"
curl.exe -sL -o $dotnetScript "https://dot.net/v1/dotnet-install.ps1"
& $dotnetScript -Channel 10.0 -InstallDir "$env:ProgramFiles\dotnet"

Write-Host ".NET 10 SDK - zakonczone." -ForegroundColor Green

# --- 2. Git for Windows (Git Bash) ---
Write-Host "`n=== Instalacja Git for Windows ===" -ForegroundColor Cyan

# Pobierz najnowsza wersje Git for Windows (64-bit)
$gitRelease = Invoke-RestMethod -Uri "https://api.github.com/repos/git-for-windows/git/releases/latest"
$gitAsset = $gitRelease.assets | Where-Object { $_.name -match "Git-.*-64-bit\.exe$" } | Select-Object -First 1
$gitInstaller = "$downloadDir\$($gitAsset.name)"

Write-Host "Pobieranie $($gitAsset.name)..."
curl.exe -sL -o $gitInstaller $gitAsset.browser_download_url

Write-Host "Instalacja Git for Windows (cicha)..."
Start-Process -FilePath $gitInstaller -ArgumentList "/VERYSILENT", "/NORESTART", "/COMPONENTS=gitlfs,assoc,assoc_sh" -Wait

Write-Host "Git for Windows - zakonczone." -ForegroundColor Green

# --- 3. Git Extensions ---
Write-Host "`n=== Instalacja Git Extensions ===" -ForegroundColor Cyan

$geRelease = Invoke-RestMethod -Uri "https://api.github.com/repos/gitextensions/gitextensions/releases/latest"
$geAsset = $geRelease.assets | Where-Object { $_.name -match "\.msi$" } | Select-Object -First 1
$geInstaller = "$downloadDir\$($geAsset.name)"

Write-Host "Pobieranie $($geAsset.name)..."
curl.exe -sL -o $geInstaller $geAsset.browser_download_url

Write-Host "Instalacja Git Extensions (cicha)..."
Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", "`"$geInstaller`"", "/quiet", "/norestart" -Wait

Write-Host "Git Extensions - zakonczone." -ForegroundColor Green

# --- 4. Azure Cosmos DB Emulator ---
Write-Host "`n=== Instalacja Azure Cosmos DB Emulator ===" -ForegroundColor Cyan

$cosmosInstaller = "$downloadDir\cosmosdb-emulator.msi"
Write-Host "Pobieranie Azure Cosmos DB Emulator..."
curl.exe -sL -o $cosmosInstaller "https://aka.ms/cosmosdb-emulator"

Write-Host "Instalacja Cosmos DB Emulator (cicha)..."
Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", "`"$cosmosInstaller`"", "/quiet", "/norestart" -Wait

Write-Host "Cosmos DB Emulator - zakonczone." -ForegroundColor Green

# --- 5. Uruchomienie Cosmos DB Emulator ---
Write-Host "`n=== Uruchomienie Cosmos DB Emulator ===" -ForegroundColor Cyan

$cosmosExe = "$env:ProgramFiles\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe"
if (Test-Path $cosmosExe) {
    Start-Process -FilePath $cosmosExe
    Write-Host "Cosmos DB Emulator uruchomiony." -ForegroundColor Green
    Write-Host "  Endpoint:    https://localhost:8081" -ForegroundColor Gray
    Write-Host "  Primary Key: C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" -ForegroundColor Gray
} else {
    Write-Warning "Nie znaleziono Cosmos DB Emulator w: $cosmosExe"
}

# --- Podsumowanie ---
Write-Host "`n=== Gotowe ===" -ForegroundColor Green
Write-Host "Zainstalowane komponenty:"
Write-Host "  - .NET 10 SDK"
Write-Host "  - Git for Windows (Git Bash)"
Write-Host "  - Git Extensions"
Write-Host "  - Azure Cosmos DB Emulator"
Write-Host "`nMoze byc konieczne ponowne uruchomienie terminala lub komputera."
