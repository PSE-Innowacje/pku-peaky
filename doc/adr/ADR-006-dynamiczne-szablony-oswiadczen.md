# ADR-006: Dynamiczne szablony oswiadczen

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Rozne typy oplat wymagaja roznych formularzy oswiadczen - kazdy z wlasnym zestawem pol (np. ilosc energii w kWh, kwoty w PLN). Formularze moga sie zmieniac w czasie bez potrzeby zmian w kodzie.

## Decyzja

Wprowadzamy encje **`DeclarationTemplate`** i **`TemplateField`** definiujace dynamiczna strukture formularzy:

**DeclarationTemplate**:
- `Name`, `Description` - identyfikacja szablonu
- `FeeType` - typ oplaty, ktorego dotyczy
- `ContractorTypes[]` - typy kontrahentow, ktorzy moga uzyc szablonu
- `Fields[]` - lista pol formularza (`TemplateField`)
- `AllowComment` - wlaczenie/wylaczenie komentarza
- `IsActive` - flaga soft-delete

**TemplateField**:
- `Number` - pozycja wyswietlania (np. "01")
- `Code` - klucz identyfikujacy pole (np. "field_01")
- `Name` - etykieta wyswietlana uzytkownikowi
- `DataType` - typ danych (Number, Text)
- `IsRequired` - walidacja wymagalnosci
- `Unit` - jednostka (np. "kWh", "PLN")

**Przechowywanie wartosci**: Oswiadczenie (`Declaration`) przechowuje wartosci pol w slowniku `FieldValues` (klucz = `Code` pola, wartosc = wprowadzona wartosc).

Administracja szablonami przez strone `/Admin/DeclarationTemplates`.

## Uzasadnienie

- Administrator moze modyfikowac formularze bez zmian w kodzie.
- Model dokumentowy Cosmos DB naturalnie obsluguje zmienny ksztalt danych.
- Slownik `FieldValues` pozwala na przechowywanie dowolnej liczby pol.
- Separacja szablonu od danych umozliwia ewolucje formularzy.

## Konsekwencje

- Brak silnego typowania wartosci pol - wszystko przechowywane jako string w slowniku.
- Walidacja pol realizowana w warstwie UI, nie w domenie.
- Zmiana szablonu nie wplywa na juz zlozone oswiadczenia (snapshot).
- Brak wersjonowania szablonow - aktualny szablon nadpisuje poprzedni.
