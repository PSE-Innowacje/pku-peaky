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

# --- Utworzenie kolekcji 'users' ---
$collectionName = "users"
Write-Host "`n=== Tworzenie kolekcji '$collectionName' ===" -ForegroundColor Cyan

$date = [DateTime]::UtcNow.ToString("R")
$resourceLink = "dbs/$databaseName"
$auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "colls" -ResourceLink $resourceLink -Date $date -Key $cosmosKey

$headers = @{
    "Authorization" = $auth
    "x-ms-version"  = "2018-12-31"
    "x-ms-date"     = $date
    "Content-Type"  = "application/json"
}

$collBody = @{
    id = $collectionName
    partitionKey = @{
        paths = @("/id")
        kind = "Hash"
    }
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "$cosmosEndpoint/$resourceLink/colls" -Method Post -Headers $headers -Body $collBody | Out-Null
Write-Host "Kolekcja '$collectionName' utworzona." -ForegroundColor Green

# --- Seedowanie uzytkownikow ---
Write-Host "`n=== Seedowanie uzytkownikow ===" -ForegroundColor Cyan

# Hash hasla 'admin123' (SHA256 -> Base64, tak jak w AuthService.HashPassword)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("admin123"))
$passwordHash = [System.Convert]::ToBase64String($hashBytes)

$users = @(
    @{
        id = "1"
        FirstName = "Administrator"
        LastName = "Systemowy"
        Email = "admin@pku.pl"
        PasswordHash = $passwordHash
        Role = 0  # Administrator
        IsActive = $true
        CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
        ContractorAbbreviation = ""
        ContractorFullName = ""
        ContractorShortName = ""
        KRS = ""
        NIP = ""
        HeadquartersAddress = ""
        ContractorCode = ""
        ContractorTypes = @()
        ContractNumber = ""
        ContractStartDate = $null
        ContractEndDate = $null
    },
    @{
        id = "2"
        FirstName = "Jan"
        LastName = "Kowalski"
        Email = "osdp@pku.pl"
        PasswordHash = $passwordHash
        Role = 1  # Kontrahent
        IsActive = $true
        CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
        ContractorAbbreviation = "OSDp-JK"
        ContractorFullName = "Jan Kowalski Sp. z o.o."
        ContractorShortName = "JK Sp."
        KRS = "0000123456"
        NIP = "1234567890"
        HeadquartersAddress = "ul. Energetyczna 1, 00-001 Warszawa"
        ContractorCode = "KON-001"
        ContractorTypes = @(0)  # OSDp
        ContractNumber = "UP/2025/001"
        ContractStartDate = "2025-01-01T00:00:00.0000000Z"
        ContractEndDate = "2026-12-31T00:00:00.0000000Z"
    },
    @{
        id = "3"
        FirstName = "Anna"
        LastName = "Nowak"
        Email = "osdn@pku.pl"
        PasswordHash = $passwordHash
        Role = 1  # Kontrahent
        IsActive = $true
        CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
        ContractorAbbreviation = "OSDn-AN"
        ContractorFullName = "Anna Nowak Energia S.A."
        ContractorShortName = "AN Energia"
        KRS = "0000234567"
        NIP = "2345678901"
        HeadquartersAddress = "ul. Przesylowa 5, 00-002 Krakow"
        ContractorCode = "KON-002"
        ContractorTypes = @(1)  # OSDn
        ContractNumber = "UP/2025/002"
        ContractStartDate = "2025-03-01T00:00:00.0000000Z"
        ContractEndDate = "2027-02-28T00:00:00.0000000Z"
    },
    @{
        id = "4"
        FirstName = "Piotr"
        LastName = "Wisniewski"
        Email = "ok@pku.pl"
        PasswordHash = $passwordHash
        Role = 1  # Kontrahent
        IsActive = $true
        CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
        ContractorAbbreviation = "OK-PW"
        ContractorFullName = "Piotr Wisniewski Odbiorca Koncowy"
        ContractorShortName = "PW Odbiorca"
        KRS = "0000345678"
        NIP = "3456789012"
        HeadquartersAddress = "ul. Odbiorcza 10, 00-003 Gdansk"
        ContractorCode = "KON-003"
        ContractorTypes = @(4)  # OdbiorcaKoncowy
        ContractNumber = "UP/2025/003"
        ContractStartDate = "2025-06-01T00:00:00.0000000Z"
        ContractEndDate = "2026-05-31T00:00:00.0000000Z"
    },
    @{
        id = "5"
        FirstName = "Maria"
        LastName = "Zielinska"
        Email = "wyt@pku.pl"
        PasswordHash = $passwordHash
        Role = 1  # Kontrahent
        IsActive = $true
        CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
        ContractorAbbreviation = "WYT-MZ"
        ContractorFullName = "Maria Zielinska Wytwarzanie Sp. z o.o."
        ContractorShortName = "MZ Wyt"
        KRS = "0000456789"
        NIP = "4567890123"
        HeadquartersAddress = "ul. Wytworcza 20, 00-004 Poznan"
        ContractorCode = "KON-004"
        ContractorTypes = @(2)  # Wytworca
        ContractNumber = "UP/2025/004"
        ContractStartDate = "2025-01-15T00:00:00.0000000Z"
        ContractEndDate = "2027-01-14T00:00:00.0000000Z"
    },
    @{
        id = "6"
        FirstName = "Tomasz"
        LastName = "Lewandowski"
        Email = "mag@pku.pl"
        PasswordHash = $passwordHash
        Role = 1  # Kontrahent
        IsActive = $true
        CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
        ContractorAbbreviation = "MAG-TL"
        ContractorFullName = "Tomasz Lewandowski Magazyn Energii S.A."
        ContractorShortName = "TL Magazyn"
        KRS = "0000567890"
        NIP = "5678901234"
        HeadquartersAddress = "ul. Magazynowa 30, 00-005 Wroclaw"
        ContractorCode = "KON-005"
        ContractorTypes = @(3)  # Magazyn
        ContractNumber = "UP/2025/005"
        ContractStartDate = "2025-04-01T00:00:00.0000000Z"
        ContractEndDate = "2026-03-31T00:00:00.0000000Z"
    }
)

$docsResourceLink = "dbs/$databaseName/colls/$collectionName"

foreach ($user in $users) {
    $date = [DateTime]::UtcNow.ToString("R")
    $auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "docs" -ResourceLink $docsResourceLink -Date $date -Key $cosmosKey

    $headers = @{
        "Authorization"        = $auth
        "x-ms-version"         = "2018-12-31"
        "x-ms-date"            = $date
        "Content-Type"         = "application/json"
        "x-ms-documentdb-partitionkey" = "[`"$($user.id)`"]"
    }

    $jsonBody = $user | ConvertTo-Json -Depth 3
    Invoke-RestMethod -Uri "$cosmosEndpoint/$docsResourceLink/docs" -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($jsonBody)) | Out-Null
    Write-Host "  Dodano uzytkownika: $($user.Email)" -ForegroundColor Gray
}

Write-Host "Seedowanie zakonczone - dodano $($users.Count) uzytkownikow." -ForegroundColor Green

# --- Podsumowanie ---
Write-Host "`n=== Gotowe ===" -ForegroundColor Green
Write-Host "Baza danych '$databaseName' z kolekcja '$collectionName' jest gotowa na: $cosmosEndpoint" -ForegroundColor Gray
