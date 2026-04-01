# ADR-002: Uproszczona Clean Architecture

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Potrzebowalismy struktury projektu zapewniajacej separacje odpowiedzialnosci, ale bez nadmiernej zlozonosci charakterystycznej dla pelnej Clean Architecture (CQRS, MediatR, itp.).

## Decyzja

Stosujemy **uproszczona Clean Architecture** z 4 warstwami:

```
PKU.Web (Prezentacja) -> PKU.Application (Interfejsy) -> PKU.Domain (Encje, Enumy, Serwisy domenowe)
                                                      -> PKU.Infrastructure (Implementacje Cosmos DB)
```

- **PKU.Domain** - encje, enumy, serwisy domenowe. Zero zaleznosci zewnetrznych.
- **PKU.Application** - wylacznie interfejsy serwisow (`IDeclarationService`, `IUserService`, `IScheduleService`, `IDeclarationTemplateService`, `IAuthService`). Brak implementacji.
- **PKU.Infrastructure** - implementacje serwisow z Cosmos DB, rejestracja DI w `DependencyInjection.cs`.
- **PKU.Web** - komponenty Blazor, kontroler uwierzytelniania, `Program.cs` z konfiguacja DI i middleware.

## Uzasadnienie

- Separacja domeny od infrastruktury pozwala na latwe testowanie i podmiane implementacji (np. migracja z in-memory na Cosmos DB - patrz ADR-012).
- Warstwa Application zawiera wylacznie interfejsy - minimalizuje coupling.
- Brak MediatR/CQRS - upraszcza kod przy zachowaniu czytelnej struktury.

## Konsekwencje

- Brak warstwy Application Services - logika koordynacji rozproszona miedzy komponentami Blazor a serwisami infrastruktury.
- Proste przeplywy danych: Komponent Blazor -> Interfejs Application -> Implementacja Infrastructure -> Cosmos DB.
- Dodanie nowego serwisu wymaga: interfejsu w Application, implementacji w Infrastructure, rejestracji w DI.
