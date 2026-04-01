# ADR-004: Uwierzytelnianie cookie z rolami

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Aplikacja wymaga kontroli dostepu - administratorzy zarzadzaja uzytkownikami, szablonami i terminarzami, a kontrahenci skladaja oswiadczenia. Potrzebowalismy prostego mechanizmu uwierzytelniania bez zewnetrznego IdP.

## Decyzja

Stosujemy **uwierzytelnianie cookie** z ASP.NET Core Identity (bez Entity Framework):

- **Hashowanie hasel**: SHA256 z kodowaniem Base64 (`AuthService.HashPassword()`)
- **Sesja**: cookie wazne 8 godzin
- **Logowanie**: POST `/account/login` (email + haslo)
- **Wylogowanie**: GET `/account/logout`
- **Role**: `Administrator`, `Kontrahent` - przechowywane jako `ClaimTypes.Role`
- **Dodatkowe claims**: `ContractorType` (lista typow kontrahenta oddzielona przecinkami)
- **Autoryzacja**: atrybut `[Authorize(Roles = "...")]` na stronach Blazor

Konta testowe: `admin@pku.pl`, `osdp@pku.pl`, `osdn@pku.pl`, `ok@pku.pl`, `wyt@pku.pl`, `mag@pku.pl` (haslo: `admin123`).

## Uzasadnienie

- Prosty mechanizm wystarczajacy dla aplikacji wewnetrznej.
- Brak zaleznosci od zewnetrznego dostawcy tozsamosci.
- Claims w cookie eliminuja potrzebe odpytywania bazy przy kazdym uzyciu.
- Dwie role pokrywaja wymagania biznesowe.

## Konsekwencje

- SHA256 nie jest najsilniejszym algorytmem hashowania hasel (brak salt, brak bcrypt/Argon2).
- Brak mechanizmu odswiezania sesji - po 8h konieczne ponowne logowanie.
- Brak SSO/OAuth - integracja z zewnetrznymi systemami wymagalaby przebudowy.
- Dane kontrahenta (typy) zduplikowane w cookie - zmiana wymaga ponownego logowania.
