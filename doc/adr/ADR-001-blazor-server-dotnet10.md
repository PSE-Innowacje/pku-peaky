# ADR-001: Wybor .NET 10 z Blazor Server jako platformy aplikacji

## Status

Zaakceptowana

## Data

2026-03-31

## Kontekst

Potrzebowalismy platformy do budowy wewnetrznej aplikacji webowej (PKU - Platforma Skladania Oswiadczen) do skladania oswiadczen rozliczeniowych. Aplikacja wymaga interaktywnego UI z formularzami, tabelami z filtrami i dashboardem. Zespol posiada kompetencje w ekosystemie .NET.

## Decyzja

Wybieramy **.NET 10** z **Blazor Server** jako platforme aplikacji.

- Blazor Server z trybem renderowania `InteractiveServer` zapewnia pelna interaktywnosc komponentow po stronie serwera przez SignalR.
- Interfejs oparty na komponentach Razor (`.razor`) z Bootstrap 5 do stylowania.
- Aplikacja nasluchuje na `http://localhost:5244`.

## Uzasadnienie

- **Blazor Server** eliminuje potrzebe pisania JavaScript - cala logika UI w C#.
- Rendering po stronie serwera upraszcza dostep do danych (brak potrzeby budowy API).
- Kompetencje zespolu w .NET pozwalaja na szybkie wdrozenie.
- Dla aplikacji wewnetrznej latency SignalR jest akceptowalne.

## Konsekwencje

- Kazda interakcja uzytkownika wymaga polaczenia SignalR z serwerem.
- Aplikacja nie dziala offline.
- Skalowalnosc ograniczona liczba jednoczesnych polaczen SignalR.
- Wymaga .NET 10 SDK do budowania i uruchamiania.
