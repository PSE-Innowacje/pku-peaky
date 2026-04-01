# ADR-003: Azure Cosmos DB jako baza danych

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Potrzebowalismy bazy danych dla encji o roznym ksztalcie (oswiadczenia z dynamicznymi polami, szablony, terminarze). Dane maja charakter dokumentowy - oswiadczenie zawiera slownik pol (`FieldValues`) o zmiennej strukturze zaleznej od szablonu.

## Decyzja

Wybieramy **Azure Cosmos DB** (NoSQL, API for NoSQL) z nastepujaca konfiguracja:

- **Kontenery**: `users`, `schedules`, `declarations`, `declaration_templates`
- **Klucz partycji**: ID encji (kazdy dokument w osobnej partycji)
- **Tryb polaczenia**: Gateway
- **Serializacja**: camelCase przez customowy `CustomCosmosSerializer` oparty na Newtonsoft.Json
- **Rejestracja**: `CosmosClient` jako Singleton w DI
- **Rozwoj lokalny**: Azure Cosmos DB Emulator (`https://localhost:8081`)

Konfiguracja w `appsettings.json`:
```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "PrimaryKey": "...",
    "DatabaseName": "pku_db",
    "UsersContainerName": "users",
    "SchedulesContainerName": "schedules",
    "DeclarationsContainerName": "declarations",
    "TemplatesContainerName": "declaration_templates"
  }
}
```

## Uzasadnienie

- Model dokumentowy naturalnie pasuje do oswiadczen z dynamicznymi polami.
- Brak potrzeby migracji schematu przy zmianach szablonow.
- Cosmos DB Emulator umozliwia pelny rozwoj lokalny bez chmury.
- Singleton `CosmosClient` zapewnia efektywne zarzadzanie polaczeniami.

## Konsekwencje

- Brak relacji miedzy dokumentami - dereferencja w kodzie aplikacji.
- Klucz partycji = ID encji oznacza brak optymalizacji zapytan cross-partition.
- Wymagany Cosmos DB Emulator do pracy lokalnej (instalacja w skrypcie `deploy/01_setup.ps1`).
- Koszty w produkcji zaleza od zuzycia RU (Request Units).
