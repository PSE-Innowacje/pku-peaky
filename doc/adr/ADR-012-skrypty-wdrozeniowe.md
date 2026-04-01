# ADR-012: Skrypty wdrozeniowe PowerShell

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Zespol potrzebowal powtarzalnego sposobu na konfiguracje srodowiska deweloperskiego - od instalacji narzedzi, przez inicjalizacje bazy danych, po uruchomienie aplikacji i testow. Srodowisko docelowe to Windows.

## Decyzja

Tworzymy **sekwencje skryptow PowerShell** w katalogu `deploy/`:

| Skrypt | Zadanie |
|--------|---------|
| `01_setup.ps1` | Instalacja .NET 10 SDK, Git for Windows, Git Extensions, Azure Cosmos DB Emulator |
| `02_init_db.ps1` | Tworzenie bazy danych i kontenerow w Cosmos DB, seedowanie uzytkownikow, szablonow i terminarzy |
| `03_run.ps1` | Uruchomienie aplikacji (`dotnet run --project src/PKU.Web`) |
| `04_unit.ps1` | Uruchomienie testow jednostkowych |
| `05_e2e.ps1` | Uruchomienie testow E2E |

Konwencje:
- Numeracja skryptow okresla kolejnosc wykonania
- Skrypty wymagaja uprawnien administratora (instalatory)
- Instalacje wykonywane cicho (silent install) z uzyciem `curl` i MSI/EXE
- Cosmos DB Emulator uruchamiany na `https://localhost:8081`

## Uzasadnienie

- PowerShell natywny dla Windows - brak dodatkowych zaleznosci.
- Numeracja zapewnia jasna kolejnosc krokow.
- Automatyczna instalacja narzedzi skraca czas onboardingu nowego czlonka zespolu.
- Seed danych w skrypcie - powtarzalny stan poczatkowy bazy.

## Konsekwencje

- Skrypty specyficzne dla Windows - brak wsparcia Linux/macOS.
- Wymagane uprawnienia administratora do instalacji.
- Seed danych w skrypcie PowerShell (nie w kodzie C#) - trudniejsze utrzymanie przy zmianach modelu.
- Brak idempotentnosci - ponowne uruchomienie moze powodowac konflikty.
