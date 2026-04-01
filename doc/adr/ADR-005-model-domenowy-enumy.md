# ADR-005: Model domenowy oparty na enumach

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Domena PKU operuje na wielu stalych zbiorach wartosci: typy oplat (przesylowe/pozaprzesylowe), typy kontrahentow, statusy oswiadczen. Potrzebowalismy sposobu na bezpieczne typowanie tych wartosci i zapobieganie blednym danym.

## Decyzja

Definiujemy **enumy C#** dla wszystkich stalych zbiorow wartosci w warstwie `PKU.Domain`:

| Enum | Wartosci |
|------|----------|
| `FeeType` | OP, OZE, OKO, OM, OZ, OJ, OR, ODO, OPPEB, OPMO (10 typow oplat) |
| `FeeCategory` | Pozaprzesylowa, Przesylowa |
| `ContractorType` | OSDp, OSDn, Wytworca, Magazyn, OdbiorcaKoncowy |
| `UserRole` | Administrator, Kontrahent |
| `DeclarationStatus` | NotSubmitted, Draft, Submitted |
| `ScheduleItemType` | DeclarationSubmit, DeclarationInvoice, DeclarationCorrection, DeclarationCorrectionInvoice |
| `DayType` | CalendarDay, BusinessDay |

Mapowanie miedzy enumami realizowane przez serwisy domenowe:
- `ContractorFeeMapping` - mapuje `ContractorType` na dozwolone `FeeType[]`
- `DeclarationNumberGenerator` - waliduje kombinacje enumow przy generowaniu numerow

## Uzasadnienie

- Kompilator wylapuje nieprawidlowe wartosci na etapie budowania.
- Enumy sa samodokumentujace - lista dozwolonych wartosci widoczna w kodzie.
- Serializacja do Cosmos DB zachowuje czytelne nazwy (camelCase).
- Latwiejsze testowanie - mozna iterowac po wszystkich wartosciach enuma.

## Konsekwencje

- Dodanie nowego typu oplaty lub kontrahenta wymaga zmiany kodu i redeploymentu.
- Mapowania miedzy enumami (np. kontrahent -> typy oplat) utrzymywane w kodzie, nie w bazie.
- Zmiana nazwy enuma wymaga migracji istniejacych dokumentow w Cosmos DB.
