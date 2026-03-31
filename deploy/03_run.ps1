# 03.ps1 - Uruchomienie projektu PKU.Web

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Uruchamianie PKU.Web ===" -ForegroundColor Cyan

dotnet run --project ..\src\PKU.Web
