# Hearthline Workspace Instructions

## Project
- Windows desktop application using .NET 8 WPF and C#.
- Keep building workflows in the existing desktop UI and preserve the local demo mode.
- REST endpoints, authentication response, and environment configuration are documented in `README.md`.
- Never store service credentials or resident passwords in source control.

## Setup Status
- [x] Requirements clarified and WPF project scaffolded.
- [x] Resident, apartment, maintenance, guest, announcement, and event workflows added.
- [x] README and API integration contract documented.
- [ ] Build and launch verification deferred: this environment has no .NET SDK.

## Validation
- Use `dotnet build` and `dotnet run` from the project root on Windows with the .NET 8 SDK installed.
