# ADR-009: Serwisy domenowe dla logiki biznesowej

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Logika biznesowa PKU obejmuje generowanie numerow oswiadczen wg okreslonego formatu oraz mapowanie typow kontrahentow na dozwolone typy oplat. Ta logika nie nalezy do konkretnej encji ani do warstwy infrastruktury.

## Decyzja

Tworzymy **serwisy domenowe** w warstwie `PKU.Domain/Services`:

### DeclarationNumberGenerator
Generuje numery oswiadczen w formacie: `OSW/{FeeType}/{ContractorAbbrev}/{Year}/{Month}/{Subperiod}/{Version}`
- Waliduje skrot kontrahenta (max 10 znakow, bez polskich znakow diakrytycznych)
- Waliduje zakres roku (2000-2100) i miesiaca (1-12)
- Obsluguje korekty - dodaje przyrostek `/NN` do numeru
- Rozroznia dozwolone typy oplat dla oswiadczen podstawowych i korekt

### ContractorFeeMapping
Mapuje `ContractorType` na tablice dozwolonych `FeeType[]`:
- Kazdy typ kontrahenta (OSDp, OSDn, Wytworca, Magazyn, OdbiorcaKoncowy) ma przypisany zbior typow oplat
- Mapowanie wykorzystywane przy automatycznym generowaniu oswiadczen na dashboardzie

## Uzasadnienie

- Logika biznesowa zamknieta w domenie - niezalezna od infrastruktury i UI.
- Serwisy domenowe latwe do testowania jednostkowego (40+ testow dla `DeclarationNumberGenerator`).
- Pojedyncza odpowiedzialnosc - kazdy serwis realizuje jedno zadanie.
- Brak zaleznosci zewnetrznych - czyste metody operujace na enumach i stringach.

## Konsekwencje

- Mapowania FeeType sa zakodowane na stale - zmiana wymaga modyfikacji kodu.
- Serwisy domenowe sa bezstanowe - moga byc uzywane jako statyczne metody.
- Walidacja numerow oswiadczen scisle powiazana z formatem PSE.
