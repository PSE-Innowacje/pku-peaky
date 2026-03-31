# 02.ps1 - Recreate bazy danych Cosmos DB

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$cosmosEndpoint = "https://localhost:8081"
$cosmosKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
$databaseName = "pku_db"

# Cosmos DB Emulator uzywa self-signed cert
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
if (-not ([System.Management.Automation.PSTypeName]'TrustAll').Type) {
    Add-Type @"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
public class TrustAll {
    public static void Enable() {
        ServicePointManager.ServerCertificateValidationCallback =
            delegate { return true; };
    }
}
"@
}
[TrustAll]::Enable()

function Get-CosmosAuthHeader {
    param(
        [string]$Verb,
        [string]$ResourceType,
        [string]$ResourceLink,
        [string]$Date,
        [string]$Key
    )
    $keyBytes = [System.Convert]::FromBase64String($Key)
    $hmac = New-Object System.Security.Cryptography.HMACSHA256(, $keyBytes)
    $payload = "$($Verb.ToLower())`n$($ResourceType.ToLower())`n$($ResourceLink.ToLower())`n$($Date.ToLower())`n`n"
    $hash = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($payload))
    $signature = [System.Convert]::ToBase64String($hash)
    [System.Web.HttpUtility]::UrlEncode("type=master&ver=1.0&sig=$signature")
}

Add-Type -AssemblyName System.Web

$date = [DateTime]::UtcNow.ToString("R")

# --- Usuniecie bazy danych jesli istnieje ---
Write-Host "=== Sprawdzanie bazy danych '$databaseName' ===" -ForegroundColor Cyan

$resourceLink = "dbs/$databaseName"
$auth = Get-CosmosAuthHeader -Verb "GET" -ResourceType "dbs" -ResourceLink $resourceLink -Date $date -Key $cosmosKey

$headers = @{
    "Authorization" = $auth
    "x-ms-version"  = "2018-12-31"
    "x-ms-date"     = $date
}

try {
    $response = Invoke-RestMethod -Uri "$cosmosEndpoint/$resourceLink" -Method Get -Headers $headers
    Write-Host "Baza danych '$databaseName' istnieje - usuwanie..." -ForegroundColor Yellow

    $date = [DateTime]::UtcNow.ToString("R")
    $auth = Get-CosmosAuthHeader -Verb "DELETE" -ResourceType "dbs" -ResourceLink $resourceLink -Date $date -Key $cosmosKey
    $headers["Authorization"] = $auth
    $headers["x-ms-date"] = $date

    Invoke-RestMethod -Uri "$cosmosEndpoint/$resourceLink" -Method Delete -Headers $headers | Out-Null
    Write-Host "Baza danych '$databaseName' usunieta." -ForegroundColor Green
} catch {
    $ex = $_.Exception
    $status = 0
    if ($null -ne $ex -and $ex.PSObject.Properties["Response"] -and $null -ne $ex.Response) {
        $status = [int]$ex.Response.StatusCode
    }
    if ($status -eq 404) {
        Write-Host "Baza danych '$databaseName' nie istnieje - pomijanie usuwania." -ForegroundColor Gray
    } else {
        throw
    }
}

# --- Utworzenie nowej bazy danych ---
Write-Host "`n=== Tworzenie bazy danych '$databaseName' ===" -ForegroundColor Cyan

$date = [DateTime]::UtcNow.ToString("R")
$auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "dbs" -ResourceLink "" -Date $date -Key $cosmosKey

$headers = @{
    "Authorization" = $auth
    "x-ms-version"  = "2018-12-31"
    "x-ms-date"     = $date
    "Content-Type"  = "application/json"
}

$body = @{ id = $databaseName } | ConvertTo-Json

Invoke-RestMethod -Uri "$cosmosEndpoint/dbs" -Method Post -Headers $headers -Body $body | Out-Null

Write-Host "Baza danych '$databaseName' utworzona." -ForegroundColor Green

# --- Podsumowanie ---
Write-Host "`n=== Gotowe ===" -ForegroundColor Green
Write-Host "Baza danych '$databaseName' jest gotowa na: $cosmosEndpoint" -ForegroundColor Gray
