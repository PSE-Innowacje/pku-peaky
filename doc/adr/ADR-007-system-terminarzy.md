# ADR-007: System terminarzy (Schedule)

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Kazdy typ oplaty i kontrahenta ma wlasne terminy skladania oswiadczen, faktur, korekt i faktur korygujacych. Terminy moga byc wyrazone w dniach kalendarzowych lub roboczych od poczatku miesiaca.

## Decyzja

Wprowadzamy encje **`Schedule`** i **`ScheduleItem`** definiujace terminy:

**Schedule**:
- `FeeType` - typ oplaty
- `ContractorType` - typ kontrahenta
- `Items[]` - lista terminow (4 typy)
- `IsActive` - flaga aktywnosci

**ScheduleItem** (4 typy terminow):
1. `DeclarationSubmit` - termin zlozenia oswiadczenia
2. `DeclarationInvoice` - termin danych fakturowych
3. `DeclarationCorrection` - termin zlozenia korekty
4. `DeclarationCorrectionInvoice` - termin faktury korygjacej

Kazdy `ScheduleItem` zawiera:
- `Days` - liczba dni od poczatku miesiaca
- `DayType` - `CalendarDay` lub `BusinessDay`

Terminarze zarzadzane przez administratora na stronie `/Admin/Schedules`. Dashboard wykorzystuje terminarze do wyswietlania alertow o zblizajacych sie i przekroczonych terminach.

## Uzasadnienie

- Rozne typy kontrahentow maja rozne terminy - potrzebna macierz FeeType x ContractorType.
- Rozroznienie dni kalendarzowych i roboczych oddaje realia biznesowe.
- Cztery typy terminow pokrywaja pelny cykl zycia oswiadczenia.
- Terminy konfigurowalne przez administratora bez zmian w kodzie.

## Konsekwencje

- Terminarz musi istniec dla kazdej kombinacji FeeType x ContractorType - brak domyslnych wartosci.
- Obliczanie dni roboczych wymaga kalendarza dni wolnych (uproszczone w obecnej implementacji).
- Dashboard musi odpytywac terminarze przy kazdym ladowaniu.
