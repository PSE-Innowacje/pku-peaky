# 05_e2e.ps1 - Uruchomienie testow E2E PKU.E2ETests

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Uruchamianie testow E2E PKU.E2ETests ===" -ForegroundColor Cyan

dotnet test ..\src\PKU.E2ETests
