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
        firstName = "Administrator"
        lastName = "Systemowy"
        email = "admin@pku.pl"
        passwordHash = $passwordHash
        role = 0  # Administrator
        isActive = $true
        createdAt = (Get-Date).ToUniversalTime().ToString("o")
        contractorAbbreviation = ""
        contractorFullName = ""
        contractorShortName = ""
        krs = ""
        nip = ""
        headquartersAddress = ""
        contractorCode = ""
        contractorTypes = @()
        contractNumber = ""
        contractStartDate = $null
        contractEndDate = $null
    },
    @{
        id = "2"
        firstName = "Jan"
        lastName = "Kowalski"
        email = "osdp@pku.pl"
        passwordHash = $passwordHash
        role = 1  # Kontrahent
        isActive = $true
        createdAt = (Get-Date).ToUniversalTime().ToString("o")
        contractorAbbreviation = "OSDp-JK"
        contractorFullName = "Jan Kowalski Sp. z o.o."
        contractorShortName = "JK Sp."
        krs = "0000123456"
        nip = "1234567890"
        headquartersAddress = "ul. Energetyczna 1, 00-001 Warszawa"
        contractorCode = "KON-001"
        contractorTypes = @(0)  # OSDp
        contractNumber = "UP/2025/001"
        contractStartDate = "2025-01-01T00:00:00.0000000Z"
        contractEndDate = "2026-12-31T00:00:00.0000000Z"
    },
    @{
        id = "3"
        firstName = "Anna"
        lastName = "Nowak"
        email = "osdn@pku.pl"
        passwordHash = $passwordHash
        role = 1  # Kontrahent
        isActive = $true
        createdAt = (Get-Date).ToUniversalTime().ToString("o")
        contractorAbbreviation = "OSDn-AN"
        contractorFullName = "Anna Nowak Energia S.A."
        contractorShortName = "AN Energia"
        krs = "0000234567"
        nip = "2345678901"
        headquartersAddress = "ul. Przesylowa 5, 00-002 Krakow"
        contractorCode = "KON-002"
        contractorTypes = @(1)  # OSDn
        contractNumber = "UP/2025/002"
        contractStartDate = "2025-03-01T00:00:00.0000000Z"
        contractEndDate = "2027-02-28T00:00:00.0000000Z"
    },
    @{
        id = "4"
        firstName = "Piotr"
        lastName = "Wisniewski"
        email = "ok@pku.pl"
        passwordHash = $passwordHash
        role = 1  # Kontrahent
        isActive = $true
        createdAt = (Get-Date).ToUniversalTime().ToString("o")
        contractorAbbreviation = "OK-PW"
        contractorFullName = "Piotr Wisniewski Odbiorca Koncowy"
        contractorShortName = "PW Odbiorca"
        krs = "0000345678"
        nip = "3456789012"
        headquartersAddress = "ul. Odbiorcza 10, 00-003 Gdansk"
        contractorCode = "KON-003"
        contractorTypes = @(4)  # OdbiorcaKoncowy
        contractNumber = "UP/2025/003"
        contractStartDate = "2025-06-01T00:00:00.0000000Z"
        contractEndDate = "2026-05-31T00:00:00.0000000Z"
    },
    @{
        id = "5"
        firstName = "Maria"
        lastName = "Zielinska"
        email = "wyt@pku.pl"
        passwordHash = $passwordHash
        role = 1  # Kontrahent
        isActive = $true
        createdAt = (Get-Date).ToUniversalTime().ToString("o")
        contractorAbbreviation = "WYT-MZ"
        contractorFullName = "Maria Zielinska Wytwarzanie Sp. z o.o."
        contractorShortName = "MZ Wyt"
        krs = "0000456789"
        nip = "4567890123"
        headquartersAddress = "ul. Wytworcza 20, 00-004 Poznan"
        contractorCode = "KON-004"
        contractorTypes = @(2)  # Wytworca
        contractNumber = "UP/2025/004"
        contractStartDate = "2025-01-15T00:00:00.0000000Z"
        contractEndDate = "2027-01-14T00:00:00.0000000Z"
    },
    @{
        id = "6"
        firstName = "Tomasz"
        lastName = "Lewandowski"
        email = "mag@pku.pl"
        passwordHash = $passwordHash
        role = 1  # Kontrahent
        isActive = $true
        createdAt = (Get-Date).ToUniversalTime().ToString("o")
        contractorAbbreviation = "MAG-TL"
        contractorFullName = "Tomasz Lewandowski Magazyn Energii S.A."
        contractorShortName = "TL Magazyn"
        krs = "0000567890"
        nip = "5678901234"
        headquartersAddress = "ul. Magazynowa 30, 00-005 Wroclaw"
        contractorCode = "KON-005"
        contractorTypes = @(3)  # Magazyn
        contractNumber = "UP/2025/005"
        contractStartDate = "2025-04-01T00:00:00.0000000Z"
        contractEndDate = "2026-03-31T00:00:00.0000000Z"
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
    Write-Host "  Dodano uzytkownika: $($user.email)" -ForegroundColor Gray
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
        userId            = "2"
        contractorType    = 0  # OSDp
        feeType           = 0  # OP
        feeCategory       = 0  # Pozaprzesylowa
        billingYear       = $prevYear
        billingMonth      = $prevMonth
        declarationNumber = "OSW/OP/OSDp-JK/$prevYear/$("{0:D2}" -f $prevMonth)/01/01"
        status            = 2  # Submitted
        submittedAt       = $prevPeriodStart.AddMonths(1).AddDays(4).ToUniversalTime().ToString("o")
        createdAt         = $prevPeriodStart.AddMonths(1).AddDays(4).ToUniversalTime().ToString("o")
        deadline          = $prevPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },
    @{
        id                = "seed-osdp-oze-prev"
        userId            = "2"
        contractorType    = 0  # OSDp
        feeType           = 1  # OZE
        feeCategory       = 0  # Pozaprzesylowa
        billingYear       = $prevYear
        billingMonth      = $prevMonth
        declarationNumber = "OSW/OZE/OSDp-JK/$prevYear/$("{0:D2}" -f $prevMonth)/01/01"
        status            = 2  # Submitted
        submittedAt       = $prevPeriodStart.AddMonths(1).AddDays(5).ToUniversalTime().ToString("o")
        createdAt         = $prevPeriodStart.AddMonths(1).AddDays(5).ToUniversalTime().ToString("o")
        deadline          = $prevPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },

    # === OSDp - biezacy miesiac: 1 robocze (OKO), 1 zlozone (OP) ===
    @{
        id                = "seed-osdp-oko-curr"
        userId            = "2"
        contractorType    = 0  # OSDp
        feeType           = 2  # OKO
        feeCategory       = 0  # Pozaprzesylowa
        billingYear       = $currentYear
        billingMonth      = $currentMonth
        declarationNumber = ""
        status            = 1  # Draft
        submittedAt       = $null
        createdAt         = (Get-Date).ToUniversalTime().ToString("o")
        deadline          = $currPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },
    @{
        id                = "seed-osdp-op-curr"
        userId            = "2"
        contractorType    = 0  # OSDp
        feeType           = 0  # OP
        feeCategory       = 0  # Pozaprzesylowa
        billingYear       = $currentYear
        billingMonth      = $currentMonth
        declarationNumber = "OSW/OP/OSDp-JK/$currentYear/$("{0:D2}" -f $currentMonth)/01/01"
        status            = 2  # Submitted
        submittedAt       = (Get-Date).AddDays(-2).ToUniversalTime().ToString("o")
        createdAt         = (Get-Date).AddDays(-2).ToUniversalTime().ToString("o")
        deadline          = $currPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },

    # === OSDn (user "3") - poprzedni miesiac: 1 zlozone ===
    @{
        id                = "seed-osdn-oze-prev"
        userId            = "3"
        contractorType    = 1  # OSDn
        feeType           = 1  # OZE
        feeCategory       = 0  # Pozaprzesylowa
        billingYear       = $prevYear
        billingMonth      = $prevMonth
        declarationNumber = "OSW/OZE/OSDn-AN/$prevYear/$("{0:D2}" -f $prevMonth)/01/01"
        status            = 2  # Submitted
        submittedAt       = $prevPeriodStart.AddMonths(1).AddDays(3).ToUniversalTime().ToString("o")
        createdAt         = $prevPeriodStart.AddMonths(1).AddDays(3).ToUniversalTime().ToString("o")
        deadline          = $prevPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
    },

    # === Wytworca (user "5") - biezacy miesiac: 1 zlozone ===
    @{
        id                = "seed-wyt-om-curr"
        userId            = "5"
        contractorType    = 2  # Wytworca
        feeType           = 3  # OM
        feeCategory       = 0  # Pozaprzesylowa
        billingYear       = $currentYear
        billingMonth      = $currentMonth
        declarationNumber = "OSW/OM/WYT-MZ/$currentYear/$("{0:D2}" -f $currentMonth)/01/01"
        status            = 2  # Submitted
        submittedAt       = (Get-Date).AddDays(-1).ToUniversalTime().ToString("o")
        createdAt         = (Get-Date).AddDays(-1).ToUniversalTime().ToString("o")
        deadline          = $currPeriodStart.AddMonths(1).AddDays(9).ToUniversalTime().ToString("o")
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
    Write-Host "  Dodano deklaracje: $($decl.id) (FeeType=$($decl.feeType), Status=$($decl.status))" -ForegroundColor Gray
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
        name            = "Oplata przejsciowa - OSDp / OSDn"
        description     = "Wzorzec oswiadczenia dla oplaty przejsciowej (OP) dla OSDp i OSDn"
        feeType         = 0  # OP
        contractorTypes = @(0, 1)  # OSDp, OSDn
        allowComment    = $true
        isActive        = $true
        createdAt       = (Get-Date).ToUniversalTime().ToString("o")
        fields          = @(
            @{ number = "1";   code = "IGDSUM"; name = "Liczba odbiorcow koncowych w gospodarstwach domowych (suma 1.1-1.3)"; dataType = "Number"; isRequired = $true; unit = "szt" }
            @{ number = "1.1"; code = "IGD1i";  name = "Zuzywajacy < 500 kWh rocznie"; dataType = "Number"; isRequired = $true; unit = "szt" }
            @{ number = "1.2"; code = "IGD2i";  name = "Zuzywajacy 500-1200 kWh rocznie"; dataType = "Number"; isRequired = $true; unit = "szt" }
            @{ number = "1.3"; code = "IGD3i";  name = "Zuzywajacy > 1200 kWh rocznie"; dataType = "Number"; isRequired = $true; unit = "szt" }
            @{ number = "2";   code = "OPSUM";  name = "Suma mocy umownych odbiorcow koncowych (suma 2.1-2.4)"; dataType = "Number (9,3)"; isRequired = $true; unit = "kW" }
            @{ number = "2.1"; code = "PnNi";   name = "Przylaczeni do sieci nN kontrahenta"; dataType = "Number (9,3)"; isRequired = $true; unit = "kW" }
            @{ number = "2.2"; code = "PSNi";   name = "Przylaczeni do sieci SN kontrahenta"; dataType = "Number (9,3)"; isRequired = $true; unit = "kW" }
            @{ number = "2.3"; code = "PWN";    name = "Przylaczeni do sieci WN/NN kontrahenta"; dataType = "Number (9,3)"; isRequired = $true; unit = "kW" }
            @{ number = "2.4"; code = "Posi";   name = "Odbiorcy >= 400 GWh, >= 60% mocy umownej, koszt EE >= 15% produkcji"; dataType = "Number (9,3)"; isRequired = $true; unit = "kW" }
        )
    },
    @{
        id              = "tpl-oze-osd"
        name            = "Oplata OZE - OSDp / OSDn"
        description     = "Wzorzec oswiadczenia dla oplaty OZE dla OSDp i OSDn"
        feeType         = 1  # OZE
        contractorTypes = @(0, 1)
        allowComment    = $true
        isActive        = $true
        createdAt       = (Get-Date).ToUniversalTime().ToString("o")
        fields          = @(
            @{ number = "1";   code = "OZESUM"; name = "Wielkosc srodkow z tytulu oplaty OZE (1.1 - 1.2)"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "1.1"; code = "OZEN";   name = "Wielkosc naleznych srodkow z tytulu oplaty OZE"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "1.2"; code = "OZEPN";  name = "Wierzytelnosci niesciagalne z poprzednich okresow"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "2";   code = "OZEE";   name = "Ilosc energii - podstawa naliczania oplaty OZE"; dataType = "Number (9,3)"; isRequired = $true; unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oze-ok-mag"
        name            = "Oplata OZE - Odbiorcy koncowi / Magazyny"
        description     = "Wzorzec dla oplaty OZE dla OK i Magazynow"
        feeType         = 1
        contractorTypes = @(4, 3)
        allowComment    = $true
        isActive        = $true
        createdAt       = (Get-Date).ToUniversalTime().ToString("o")
        fields          = @(
            @{ number = "1"; code = "OZEil"; name = "Ilosc energii - podstawa naliczania oplaty OZE"; dataType = "Number (9,3)"; isRequired = $true; unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oze-wyt"
        name            = "Oplata OZE - Wytworca"
        description     = "Wzorzec dla oplaty OZE dla wytworcow"
        feeType         = 1
        contractorTypes = @(2)
        allowComment    = $true
        isActive        = $true
        createdAt       = (Get-Date).ToUniversalTime().ToString("o")
        fields          = @(
            @{ number = "1"; code = "OZEil"; name = "Planowana ilosc energii - podstawa naliczania oplaty OZE"; dataType = "Number (9,3)"; isRequired = $true; unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oko-osd"
        name            = "Oplata kogeneracyjna - OSDp / OSDn"
        description     = "Wzorzec dla oplaty kogeneracyjnej dla OSDp i OSDn"
        feeType         = 2
        contractorTypes = @(0, 1)
        allowComment    = $true
        isActive        = $true
        createdAt       = (Get-Date).ToUniversalTime().ToString("o")
        fields          = @(
            @{ number = "1";   code = "OKOSUM"; name = "Wielkosc srodkow z tytulu oplaty kogeneracyjnej (1.1 - 1.2)"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "1.1"; code = "OKON";   name = "Wielkosc naleznych srodkow"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "1.2"; code = "OKOPN";  name = "Wierzytelnosci niesciagalne z poprzednich okresow"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "1.3"; code = "OKOO";   name = "Wielkosc pobranych srodkow"; dataType = "Number (12,2)"; isRequired = $true; unit = "zl" }
            @{ number = "2";   code = "OKOE";   name = "Ilosc energii - podstawa naliczania oplaty kogeneracyjnej"; dataType = "Number (9,3)"; isRequired = $true; unit = "MWh" }
        )
    },
    @{
        id              = "tpl-oko-ok-wyt-mag"
        name            = "Oplata kogeneracyjna - OK / Wyt / Mag"
        description     = "Wzorzec dla oplaty kogeneracyjnej dla OK, wytworcow i magazynow"
        feeType         = 2
        contractorTypes = @(4, 2, 3)
        allowComment    = $true
        isActive        = $true
        createdAt       = (Get-Date).ToUniversalTime().ToString("o")
        fields          = @(
            @{ number = "1"; code = "OKOE"; name = "Ilosc energii - podstawa naliczania oplaty kogeneracyjnej"; dataType = "Number (9,3)"; isRequired = $false; unit = "MWh" }
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
    Write-Host "  Dodano wzorzec: $($tpl.name)" -ForegroundColor Gray
}

Write-Host "Seedowanie zakonczone - dodano $($templates.Count) wzorcow." -ForegroundColor Green

# --- Podsumowanie ---
Write-Host "`n=== Gotowe ===" -ForegroundColor Green
Write-Host "Baza danych '$databaseName' z kolekcjami '$collectionName', '$declCollectionName', '$schedCollectionName' i '$tplCollectionName' jest gotowa na: $cosmosEndpoint" -ForegroundColor Gray
