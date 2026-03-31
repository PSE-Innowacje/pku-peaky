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

# --- Utworzenie kolekcji 'declarations' ---
$declCollectionName = "declarations"
Write-Host "`n=== Tworzenie kolekcji '$declCollectionName' ===" -ForegroundColor Cyan

$date = [DateTime]::UtcNow.ToString("R")
$auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "colls" -ResourceLink $resourceLink -Date $date -Key $cosmosKey

$headers = @{
    "Authorization" = $auth
    "x-ms-version"  = "2018-12-31"
    "x-ms-date"     = $date
    "Content-Type"  = "application/json"
}

$declCollBody = @{
    id = $declCollectionName
    partitionKey = @{
        paths = @("/id")
        kind = "Hash"
    }
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "$cosmosEndpoint/$resourceLink/colls" -Method Post -Headers $headers -Body $declCollBody | Out-Null
Write-Host "Kolekcja '$declCollectionName' utworzona." -ForegroundColor Green

# --- Seedowanie deklaracji ---
Write-Host "`n=== Seedowanie deklaracji ===" -ForegroundColor Cyan

# Enum values (odpowiadaja C# enum):
# FeeType:           OP=0, OZE=1, OKO=2, OM=3, OZ=4, OJ=5, OR=6, ODO=7, OPPEB=8, OPMO=9
# FeeCategory:       Pozaprzesylowa=0, Przesylowa=1
# DeclarationStatus: NotSubmitted=0, Draft=1, Submitted=2
# ContractorType:    OSDp=0, OSDn=1, Wytworca=2, Magazyn=3, OdbiorcaKoncowy=4

$now = Get-Date
$currentYear = $now.Year
$currentMonth = $now.Month

if ($currentMonth -eq 1) {
    $prevMonth = 12
    $prevYear = $currentYear - 1
} else {
    $prevMonth = $currentMonth - 1
    $prevYear = $currentYear
}

$prevPeriodStart = Get-Date -Year $prevYear -Month $prevMonth -Day 1 -Hour 0 -Minute 0 -Second 0
$currPeriodStart = Get-Date -Year $currentYear -Month $currentMonth -Day 1 -Hour 0 -Minute 0 -Second 0

$declarations = @(
    # === OSDp (user "2") - poprzedni miesiac: 2 zlozone ===
    @{
        id                = "seed-osdp-op-prev"
        UserId            = "2"
        ContractorType    = 0  # OSDp
        FeeType           = 0  # OP
        FeeCategory       = 0  # Pozaprzesylowa
        BillingYear       = $prevYear
        BillingMonth      = $prevMonth
        DeclarationNumber = "OSW/OP/OSDp-JK/$prevYear/$("{0:D2}" -f $prevMonth)/01/01"
        Status            = 2  # Submitted
        SubmittedAt       = $prevPeriodStart.AddMonths(1).AddDays(4).ToUniversalTime().ToString("o")
        CreatedAt         = $prevPeriodStart.AddMonths(1).AddDays(4).ToUniversalTime().ToString("o")
        Deadline          = $prevPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },
    @{
        id                = "seed-osdp-oze-prev"
        UserId            = "2"
        ContractorType    = 0  # OSDp
        FeeType           = 1  # OZE
        FeeCategory       = 0  # Pozaprzesylowa
        BillingYear       = $prevYear
        BillingMonth      = $prevMonth
        DeclarationNumber = "OSW/OZE/OSDp-JK/$prevYear/$("{0:D2}" -f $prevMonth)/01/01"
        Status            = 2  # Submitted
        SubmittedAt       = $prevPeriodStart.AddMonths(1).AddDays(5).ToUniversalTime().ToString("o")
        CreatedAt         = $prevPeriodStart.AddMonths(1).AddDays(5).ToUniversalTime().ToString("o")
        Deadline          = $prevPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },

    # === OSDp - biezacy miesiac: 1 robocze (OKO), 1 zlozone (OP) ===
    @{
        id                = "seed-osdp-oko-curr"
        UserId            = "2"
        ContractorType    = 0  # OSDp
        FeeType           = 2  # OKO
        FeeCategory       = 0  # Pozaprzesylowa
        BillingYear       = $currentYear
        BillingMonth      = $currentMonth
        DeclarationNumber = ""
        Status            = 1  # Draft
        SubmittedAt       = $null
        CreatedAt         = (Get-Date).ToUniversalTime().ToString("o")
        Deadline          = $currPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },
    @{
        id                = "seed-osdp-op-curr"
        UserId            = "2"
        ContractorType    = 0  # OSDp
        FeeType           = 0  # OP
        FeeCategory       = 0  # Pozaprzesylowa
        BillingYear       = $currentYear
        BillingMonth      = $currentMonth
        DeclarationNumber = "OSW/OP/OSDp-JK/$currentYear/$("{0:D2}" -f $currentMonth)/01/01"
        Status            = 2  # Submitted
        SubmittedAt       = (Get-Date).AddDays(-2).ToUniversalTime().ToString("o")
        CreatedAt         = (Get-Date).AddDays(-2).ToUniversalTime().ToString("o")
        Deadline          = $currPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },

    # === OSDn (user "3") - poprzedni miesiac: 1 zlozone ===
    @{
        id                = "seed-osdn-oze-prev"
        UserId            = "3"
        ContractorType    = 1  # OSDn
        FeeType           = 1  # OZE
        FeeCategory       = 0  # Pozaprzesylowa
        BillingYear       = $prevYear
        BillingMonth      = $prevMonth
        DeclarationNumber = "OSW/OZE/OSDn-AN/$prevYear/$("{0:D2}" -f $prevMonth)/01/01"
        Status            = 2  # Submitted
        SubmittedAt       = $prevPeriodStart.AddMonths(1).AddDays(3).ToUniversalTime().ToString("o")
        CreatedAt         = $prevPeriodStart.AddMonths(1).AddDays(3).ToUniversalTime().ToString("o")
        Deadline          = $prevPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },

    # === Wytworca (user "5") - biezacy miesiac: 1 zlozone ===
    @{
        id                = "seed-wyt-om-curr"
        UserId            = "5"
        ContractorType    = 2  # Wytworca
        FeeType           = 3  # OM
        FeeCategory       = 0  # Pozaprzesylowa
        BillingYear       = $currentYear
        BillingMonth      = $currentMonth
        DeclarationNumber = "OSW/OM/WYT-MZ/$currentYear/$("{0:D2}" -f $currentMonth)/01/01"
        Status            = 2  # Submitted
        SubmittedAt       = (Get-Date).AddDays(-1).ToUniversalTime().ToString("o")
        CreatedAt         = (Get-Date).AddDays(-1).ToUniversalTime().ToString("o")
        Deadline          = $currPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    }
)

$declDocsResourceLink = "dbs/$databaseName/colls/$declCollectionName"

foreach ($decl in $declarations) {
    $date = [DateTime]::UtcNow.ToString("R")
    $auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "docs" -ResourceLink $declDocsResourceLink -Date $date -Key $cosmosKey

    $headers = @{
        "Authorization"                = $auth
        "x-ms-version"                 = "2018-12-31"
        "x-ms-date"                    = $date
        "Content-Type"                 = "application/json"
        "x-ms-documentdb-partitionkey" = "[`"$($decl.id)`"]"
    }

    $jsonBody = $decl | ConvertTo-Json -Depth 3
    Invoke-RestMethod -Uri "$cosmosEndpoint/$declDocsResourceLink/docs" -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($jsonBody)) | Out-Null
    Write-Host "  Dodano deklaracje: $($decl.id) (FeeType=$($decl.FeeType), Status=$($decl.Status))" -ForegroundColor Gray
}

Write-Host "Seedowanie zakonczone - dodano $($declarations.Count) deklaracji." -ForegroundColor Green

# --- Utworzenie kolekcji 'schedules' ---
$schedCollectionName = "schedules"
Write-Host "`n=== Tworzenie kolekcji '$schedCollectionName' ===" -ForegroundColor Cyan

$date = [DateTime]::UtcNow.ToString("R")
$auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "colls" -ResourceLink $resourceLink -Date $date -Key $cosmosKey

$headers = @{
    "Authorization" = $auth
    "x-ms-version"  = "2018-12-31"
    "x-ms-date"     = $date
    "Content-Type"  = "application/json"
}

$schedCollBody = @{
    id = $schedCollectionName
    partitionKey = @{
        paths = @("/id")
        kind = "Hash"
    }
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "$cosmosEndpoint/$resourceLink/colls" -Method Post -Headers $headers -Body $schedCollBody | Out-Null
Write-Host "Kolekcja '$schedCollectionName' utworzona." -ForegroundColor Green

# --- Seedowanie terminarzy ---
Write-Host "`n=== Seedowanie terminarzy ===" -ForegroundColor Cyan

# Enum values (odpowiadaja C# enum):
# FeeType:          OP=0, OZE=1, OKO=2, OM=3, OZ=4, OJ=5, OR=6, ODO=7, OPPEB=8, OPMO=9
# ContractorType:   OSDp=0, OSDn=1, Wytworca=2, Magazyn=3, OdbiorcaKoncowy=4
# ScheduleItemType: DeclarationSubmit=0, DeclarationInvoice=1, DeclarationCorrection=2, DeclarationCorrectionInvoice=3
# DayType:          CalendarDay=0, BusinessDay=1

$schedules = @(
    @{
        id             = "seed-schedule-op-osdp"
        feeType        = 0  # OP
        contractorType = 0  # OSDp
        items          = @(
            @{ itemType = 0; days = 10; dayType = 1 }  # DeclarationSubmit, BusinessDay
            @{ itemType = 1; days = 15; dayType = 0 }  # DeclarationInvoice, CalendarDay
            @{ itemType = 2; days = 20; dayType = 1 }  # DeclarationCorrection, BusinessDay
            @{ itemType = 3; days = 25; dayType = 0 }  # DeclarationCorrectionInvoice, CalendarDay
        )
        isActive       = $true
    },
    @{
        id             = "seed-schedule-oze-wytworca"
        feeType        = 1  # OZE
        contractorType = 2  # Wytworca
        items          = @(
            @{ itemType = 0; days = 7;  dayType = 0 }  # DeclarationSubmit, CalendarDay
            @{ itemType = 1; days = 12; dayType = 0 }  # DeclarationInvoice, CalendarDay
            @{ itemType = 2; days = 17; dayType = 1 }  # DeclarationCorrection, BusinessDay
            @{ itemType = 3; days = 22; dayType = 0 }  # DeclarationCorrectionInvoice, CalendarDay
        )
        isActive       = $true
    }
)

$schedDocsResourceLink = "dbs/$databaseName/colls/$schedCollectionName"

foreach ($sched in $schedules) {
    $date = [DateTime]::UtcNow.ToString("R")
    $auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "docs" -ResourceLink $schedDocsResourceLink -Date $date -Key $cosmosKey

    $headers = @{
        "Authorization"                = $auth
        "x-ms-version"                 = "2018-12-31"
        "x-ms-date"                    = $date
        "Content-Type"                 = "application/json"
        "x-ms-documentdb-partitionkey" = "[`"$($sched.id)`"]"
    }

    $jsonBody = $sched | ConvertTo-Json -Depth 3
    Invoke-RestMethod -Uri "$cosmosEndpoint/$schedDocsResourceLink/docs" -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($jsonBody)) | Out-Null
    Write-Host "  Dodano terminarz: $($sched.id) (FeeType=$($sched.feeType), ContractorType=$($sched.contractorType))" -ForegroundColor Gray
}

Write-Host "Seedowanie zakonczone - dodano $($schedules.Count) terminarzy." -ForegroundColor Green

# --- Utworzenie kolekcji 'declaration_templates' ---
$tplCollectionName = "declaration_templates"
Write-Host "`n=== Tworzenie kolekcji '$tplCollectionName' ===" -ForegroundColor Cyan

$date = [DateTime]::UtcNow.ToString("R")
$auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "colls" -ResourceLink $resourceLink -Date $date -Key $cosmosKey

$headers = @{
    "Authorization" = $auth
    "x-ms-version"  = "2018-12-31"
    "x-ms-date"     = $date
    "Content-Type"  = "application/json"
}

$tplCollBody = @{
    id = $tplCollectionName
    partitionKey = @{
        paths = @("/id")
        kind = "Hash"
    }
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "$cosmosEndpoint/$resourceLink/colls" -Method Post -Headers $headers -Body $tplCollBody | Out-Null
Write-Host "Kolekcja '$tplCollectionName' utworzona." -ForegroundColor Green

# --- Seedowanie wzorcow oswiadczen ---
Write-Host "`n=== Seedowanie wzorcow oswiadczen ===" -ForegroundColor Cyan

$templates = @(
    @{
        id              = "tpl-op-osdp-osdn"
        Name            = "Oplata przejsciowa - OSDp / OSDn"
        Description     = "Wzorzec oswiadczenia dla oplaty przejsciowej (OP) dla OSDp i OSDn"
        FeeType         = 0  # OP
        ContractorTypes = @(0, 1)  # OSDp, OSDn
        AllowComment    = $true
        IsActive        = $true
        CreatedAt       = (Get-Date).ToUniversalTime().ToString("o")
        Fields          = @(
            @{ Number = "1";   Code = "IGDSUM"; Name = "Liczba odbiorcow koncowych w gospodarstwach domowych (suma 1.1-1.3)"; DataType = "Number"; IsRequired = $true; Unit = "szt" }
            @{ Number = "1.1"; Code = "IGD1i";  Name = "Zuzywajacy < 500 kWh rocznie"; DataType = "Number"; IsRequired = $true; Unit = "szt" }
            @{ Number = "1.2"; Code = "IGD2i";  Name = "Zuzywajacy 500-1200 kWh rocznie"; DataType = "Number"; IsRequired = $true; Unit = "szt" }
            @{ Number = "1.3"; Code = "IGD3i";  Name = "Zuzywajacy > 1200 kWh rocznie"; DataType = "Number"; IsRequired = $true; Unit = "szt" }
            @{ Number = "2";   Code = "OPSUM";  Name = "Suma mocy umownych odbiorcow koncowych (suma 2.1-2.4)"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "kW" }
            @{ Number = "2.1"; Code = "PnNi";   Name = "Przylaczeni do sieci nN kontrahenta"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "kW" }
            @{ Number = "2.2"; Code = "PSNi";   Name = "Przylaczeni do sieci SN kontrahenta"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "kW" }
            @{ Number = "2.3"; Code = "PWN";    Name = "Przylaczeni do sieci WN/NN kontrahenta"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "kW" }
            @{ Number = "2.4"; Code = "Posi";   Name = "Odbiorcy >= 400 GWh, >= 60% mocy umownej, koszt EE >= 15% produkcji"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "kW" }
        )
    },
    @{
        id              = "tpl-oze-osd"
        Name            = "Oplata OZE - OSDp / OSDn"
        Description     = "Wzorzec oswiadczenia dla oplaty OZE dla OSDp i OSDn"
        FeeType         = 1  # OZE
        ContractorTypes = @(0, 1)
        AllowComment    = $true
        IsActive        = $true
        CreatedAt       = (Get-Date).ToUniversalTime().ToString("o")
        Fields          = @(
            @{ Number = "1";   Code = "OZESUM"; Name = "Wielkosc srodkow z tytulu oplaty OZE (1.1 - 1.2)"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "1.1"; Code = "OZEN";   Name = "Wielkosc naleznych srodkow z tytulu oplaty OZE"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "1.2"; Code = "OZEPN";  Name = "Wierzytelnosci niesciagalne z poprzednich okresow"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "2";   Code = "OZEE";   Name = "Ilosc energii - podstawa naliczania oplaty OZE"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oze-ok-mag"
        Name            = "Oplata OZE - Odbiorcy koncowi / Magazyny"
        Description     = "Wzorzec dla oplaty OZE dla OK i Magazynow"
        FeeType         = 1
        ContractorTypes = @(4, 3)
        AllowComment    = $true
        IsActive        = $true
        CreatedAt       = (Get-Date).ToUniversalTime().ToString("o")
        Fields          = @(
            @{ Number = "1"; Code = "OZEil"; Name = "Ilosc energii - podstawa naliczania oplaty OZE"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oze-wyt"
        Name            = "Oplata OZE - Wytworca"
        Description     = "Wzorzec dla oplaty OZE dla wytworcow"
        FeeType         = 1
        ContractorTypes = @(2)
        AllowComment    = $true
        IsActive        = $true
        CreatedAt       = (Get-Date).ToUniversalTime().ToString("o")
        Fields          = @(
            @{ Number = "1"; Code = "OZEil"; Name = "Planowana ilosc energii - podstawa naliczania oplaty OZE"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oko-osd"
        Name            = "Oplata kogeneracyjna - OSDp / OSDn"
        Description     = "Wzorzec dla oplaty kogeneracyjnej dla OSDp i OSDn"
        FeeType         = 2
        ContractorTypes = @(0, 1)
        AllowComment    = $true
        IsActive        = $true
        CreatedAt       = (Get-Date).ToUniversalTime().ToString("o")
        Fields          = @(
            @{ Number = "1";   Code = "OKOSUM"; Name = "Wielkosc srodkow z tytulu oplaty kogeneracyjnej (1.1 - 1.2)"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "1.1"; Code = "OKON";   Name = "Wielkosc naleznych srodkow"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "1.2"; Code = "OKOPN";  Name = "Wierzytelnosci niesciagalne z poprzednich okresow"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "1.3"; Code = "OKOO";   Name = "Wielkosc pobranych srodkow"; DataType = "Number (12,2)"; IsRequired = $true; Unit = "zl" }
            @{ Number = "2";   Code = "OKOE";   Name = "Ilosc energii - podstawa naliczania oplaty kogeneracyjnej"; DataType = "Number (9,3)"; IsRequired = $true; Unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oko-ok-wyt-mag"
        Name            = "Oplata kogeneracyjna - OK / Wyt / Mag"
        Description     = "Wzorzec dla oplaty kogeneracyjnej dla OK, wytworcow i magazynow"
        FeeType         = 2
        ContractorTypes = @(4, 2, 3)
        AllowComment    = $true
        IsActive        = $true
        CreatedAt       = (Get-Date).ToUniversalTime().ToString("o")
        Fields          = @(
            @{ Number = "1"; Code = "OKOE"; Name = "Ilosc energii - podstawa naliczania oplaty kogeneracyjnej"; DataType = "Number (9,3)"; IsRequired = $false; Unit = "MWh" }
        )
    }
)

$tplDocsResourceLink = "dbs/$databaseName/colls/$tplCollectionName"

foreach ($tpl in $templates) {
    $date = [DateTime]::UtcNow.ToString("R")
    $auth = Get-CosmosAuthHeader -Verb "POST" -ResourceType "docs" -ResourceLink $tplDocsResourceLink -Date $date -Key $cosmosKey

    $headers = @{
        "Authorization"                = $auth
        "x-ms-version"                 = "2018-12-31"
        "x-ms-date"                    = $date
        "Content-Type"                 = "application/json"
        "x-ms-documentdb-partitionkey" = "[`"$($tpl.id)`"]"
    }

    $jsonBody = $tpl | ConvertTo-Json -Depth 5
    Invoke-RestMethod -Uri "$cosmosEndpoint/$tplDocsResourceLink/docs" -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($jsonBody)) | Out-Null
    Write-Host "  Dodano wzorzec: $($tpl.Name)" -ForegroundColor Gray
}

Write-Host "Seedowanie zakonczone - dodano $($templates.Count) wzorcow." -ForegroundColor Green

# --- Podsumowanie ---
Write-Host "`n=== Gotowe ===" -ForegroundColor Green
Write-Host "Baza danych '$databaseName' z kolekcjami '$collectionName', '$declCollectionName', '$schedCollectionName' i '$tplCollectionName' jest gotowa na: $cosmosEndpoint" -ForegroundColor Gray
