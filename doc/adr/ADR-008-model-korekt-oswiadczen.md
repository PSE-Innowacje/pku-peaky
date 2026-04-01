# ADR-008: Model korekt oswiadczen

## Status

Zaakceptowana

## Data

2026-04-01

## Kontekst

Po zlozeniu oswiadczenia kontrahent moze potrzebowac dokonac korekty. Potrzebowalismy modelu, ktory zachowuje historie zmian i pozwala na identyfikacje korekt.

## Decyzja

Korekty realizowane jako **nowe dokumenty `Declaration`** powiazane z oryginalem:

- `CorrectionNumber` - numer kolejny korekty (0 = oryginalne oswiadczenie, 1 = pierwsza korekta, itd.)
- `OriginalDeclarationId` - referencja do oryginalnego oswiadczenia

Tworzenie korekty:
1. Uzytkownik klika "Korekta" przy zlozonym oswiadczeniu na liscie (`Declarations.razor`)
2. System tworzy nowy dokument `Declaration` z inkrementowanym `CorrectionNumber`
3. Nowy dokument linkuje do orginalu przez `OriginalDeclarationId`
4. Formularz korekty otwiera sie z pustymi polami do wypelnienia

Numer korekty wyswietlany na liscie oswiadczen. Dashboard pokazuje oddzielna karte z podsumowaniem korekt.

## Uzasadnienie

- Kazda korekta to osobny dokument - pelna historia zmian zachowana.
- Prosty model bez skomplikowanego wersjonowania.
- Inkrementalny `CorrectionNumber` zapewnia jednoznaczna identyfikacje.
- Referencja do orginalu pozwala na nawigacje miedzy wersjami.

## Konsekwencje

- Korekta nie kopiuje wartosci pol z orginalu - uzytkownik wypelnia formularz od nowa.
- Brak mechanizmu diff miedzy oryginalem a korekta.
- Lista oswiadczen moze zawierac wiele dokumentow dla tego samego okresu i typu oplaty.
- Numeracja korekt realizowana przez `DeclarationNumberGenerator` (przyrostek `/NN`).
