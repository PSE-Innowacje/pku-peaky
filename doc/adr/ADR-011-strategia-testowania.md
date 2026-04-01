# ADR-011: Strategia testowania (xUnit + Playwright)

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Potrzebowalismy strategii testowania pokrywajacej zarowno logike domenowa jak i interakcje uzytkownika z aplikacja. Zespol pracowal rownolegle nad roznymi czesciami systemu.

## Decyzja

Stosujemy **dwa poziomy testow** w oddzielnych projektach:

### Testy jednostkowe (PKU.Tests)
- Framework: **xUnit** z atrybutami `[Fact]` i `[Theory]`
- Zakres: serwisy domenowe (`DeclarationNumberGenerator`, `ContractorFeeMapping`)
- Uruchamianie: `dotnet test src/PKU.Tests`
- Mozliwosc filtrowania: `dotnet test --filter "FullyQualifiedName~TestMethodName"`

### Testy E2E (PKU.E2ETests)
- Framework: **Playwright** dla .NET
- Przegladarka: Chromium (headless)
- Bazowy URL: `http://localhost:5244`
- `PlaywrightFixture` - wspolny fixture zarzadzajacy przegladarka i uwierzytelnianiem
- Metoda `CreateAuthenticatedPageAsync(email, password)` - loguje uzytkownika i czeka na przekierowanie
- Testy uruchamiane seryjnie (nie rownolegle) - wspoldzielony stan aplikacji
- Wymagana uruchomiona aplikacja na localhost:5244

Pokrycie E2E:
- `LoginTests` - logowanie, wylogowanie, dostep nieautoryzowany
- `DashboardTests` - widoki admina i kontrahenta
- `AdminTests` - panel uzytkownikow, kontrola dostepu
- `HomeTests` - strona glowna, nawigacja

## Uzasadnienie

- Testy jednostkowe dla czystej logiki domenowej - szybkie, deterministyczne.
- Testy E2E dla pelnych przeplywow uzytkownika - wykrywaja problemy integracyjne.
- Playwright zapewnia stabilne testy przeglgdarkowe z API async/await.
- Oddzielne projekty - mozna uruchamiac niezaleznie.

## Konsekwencje

- Brak testow integracyjnych warstwy Infrastructure (serwisy Cosmos DB nietestowane jednostkowo).
- Testy E2E wymagaja uruchomionej aplikacji i bazy danych - wolniejsze w CI/CD.
- Seryjne wykonanie testow E2E wydluza czas pelnego przebiegu.
