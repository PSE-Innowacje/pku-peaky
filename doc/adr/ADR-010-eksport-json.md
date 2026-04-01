# ADR-010: Eksport oswiadczen do JSON

## Status

Zaakceptowana

## Data

2026-04-01

## Kontekst

Zlozone oswiadczenia musza byc eksportowane do formatu umozliwiajacego przetwarzanie przez systemy zewnetrzne. Potrzebowalismy prostego formatu eksportu zachowujacego pelna strukture oswiadczenia.

## Decyzja

Eksport oswiadczen realizowany jako **plik JSON** z pelna serializacja obiektu `Declaration`:

- Serializacja przez `System.Text.Json` (nie Newtonsoft.Json uzywany w Cosmos DB)
- Pretty-print z wcienciami (`WriteIndented = true`)
- Kodowanie `UnsafeRelaxedJsonEscaping` dla zachowania polskich znakow
- Metoda `ExportToJson()` w `CosmosDeclarationService` zwraca `byte[]`
- Eksport dostepny z listy oswiadczen (`Declarations.razor`) - pobranie pojedynczego lub wielu oswiadczen
- Eksport oczekujacych oswiadczen przez `GetPendingAsync()` dla przetwarzania wsadowego

## Uzasadnienie

- JSON to uniwersalny format integracyjny.
- Pelna serializacja obiektu zachowuje wszystkie dane bez transformacji.
- Pretty-print ulatwia debugowanie i reczna inspekcje.
- `byte[]` umozliwia bezposredni download z przegladarki.

## Konsekwencje

- Format eksportu zwiazany ze struktura encji `Declaration` - zmiana encji zmienia format.
- Brak dedykowanego schematu eksportu (DTO) - eksportowane sa tez pola wewnetrzne.
- Dwa serializery w projekcie: Newtonsoft.Json (Cosmos DB) i System.Text.Json (eksport).
