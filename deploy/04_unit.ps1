# 04_unit.ps1 - Uruchomienie testow jednostkowych PKU.Tests

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Uruchamianie testow jednostkowych PKU.Tests ===" -ForegroundColor Cyan

dotnet test ..\src\PKU.Tests
