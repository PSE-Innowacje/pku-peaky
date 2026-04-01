# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PKU (Platforma Skladania Oswiadczen) - web application for submitting declarations for utility fee (przesylowe) and non-utility fee (pozaprzesylowe) settlements. Polish-language UI.

## Commands

```bash
# Run the app (listens on http://localhost:5244)
dotnet run --project src/PKU.Web

# Run unit tests
dotnet test src/PKU.Tests

# Run a single test by name
dotnet test src/PKU.Tests --filter "FullyQualifiedName~TestMethodName"

# Run E2E tests (requires app running on localhost:5244)
dotnet test src/PKU.E2ETests

# Build without running
dotnet build src/PKU.Web
```

## Architecture

Simplified Clean Architecture (.NET 10, Blazor Server, Azure Cosmos DB):

- **PKU.Web** - Blazor Server UI, Razor components, cookie-based auth with roles (Administrator, Kontrahent). Entry point is `Program.cs` for DI and middleware setup.
- **PKU.Application** - Service interfaces only (`IDeclarationService`, `IUserService`, `IScheduleService`, `IDeclarationTemplateService`, `IAuthService`). No implementations here.
- **PKU.Domain** - Entities (`User`, `Declaration`, `DeclarationTemplate`, `Schedule`, `ScheduleItem`, `TemplateField`), enums (`FeeType`, `UserRole`, `DeclarationStatus`, `ContractorType`, `FeeCategory`, `DayType`), and domain services (`DeclarationNumberGenerator`, `ContractorFeeMapping`).
- **PKU.Infrastructure** - Cosmos DB service implementations (`CosmosUserService`, `CosmosDeclarationService`, etc.) and `AuthService`. DI registration in `DependencyInjection.cs`.

Data flows: Blazor components -> Application interfaces -> Infrastructure Cosmos services -> Azure Cosmos DB.

## Key Conventions

- No business logic in Blazor components - use domain services and application layer
- Validate with FluentValidation
- async/await everywhere
- Keep methods under 40 lines
- Prefer composition over inheritance
- Use enums instead of magic strings
- Nullable reference types enabled across all projects

## Database

Azure Cosmos DB with containers: `users`, `schedules`, `declarations`, `declaration_templates`. Connection configured in `appsettings.json` under `CosmosDb` section. Local development uses the Cosmos DB Emulator (https://localhost:8081).

## Testing

- **Unit tests**: xUnit in `PKU.Tests`. Currently covers `DeclarationNumberGenerator` (40+ test cases).
- **E2E tests**: Playwright in `PKU.E2ETests`. Uses `PlaywrightFixture` for authenticated browser sessions against localhost:5244.
