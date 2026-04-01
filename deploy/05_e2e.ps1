# 05_e2e.ps1 - Uruchomienie testow E2E PKU.E2ETests

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Uruchamianie testow E2E PKU.E2ETests ===" -ForegroundColor Cyan

..\src\PKU.E2ETests\bin\Debug\net10.0\playwright.ps1 install

dotnet test ..\src\PKU.E2ETests
