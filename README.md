# ⚡ PKU — Platforma składania oświadczeń

## 📋 Opis
Aplikacja webowa do składania oświadczeń dla rozliczeń opłat przesyłowych i pozaprzesyłowych.

---

## 🎯 Cel
Wsparcie procesu rozliczeń dla Umów Przesyłowych.

---

## 🧱 Stack technologiczny

- .NET 10
- Blazor Server
- Azure Cosmos DB (NoSQL)
- FluentValidation

---

## 🧠 Architektura

Uproszczona Clean Architecture:

/src
  /PKU.Web
  /PKU.Application
  /PKU.Domain
  /PKU.Infrastructure

Zasady:
- brak logiki biznesowej w UI
- logika w Application
- dostęp do danych w Infrastructure

## ▶️ Uruchomienie

dotnet run --project src/PKU.Web

---

## 📏 Zasady

- async/await everywhere
- małe metody
- brak duplikacji
- walidacja przez FluentValidation

---

## 🤖 AI Rules

Follow AI_RULES.md