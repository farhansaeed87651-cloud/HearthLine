# Hearthline

Hearthline is a Windows desktop building-management app built with WPF and .NET 8. It includes an admin workspace and a resident view for a 150-apartment building.

## Features

- Resident directory and apartment amenity condition tracking for showers, air conditioning, heating, and appliances.
- Maintenance reporting and work-order queue.
- Resident guest invitations and a building guest-pass list.
- Building-wide announcements and community events.
- Search across the current list, role-aware screens, and an operations dashboard.
- Demo mode with generated sample residents and apartments when no service URL is configured.

## Run

Install the .NET 8 SDK on Windows, then run these commands from the project directory:

```powershell
dotnet restore
dotnet run
```

To compile without launching the desktop window:

```powershell
dotnet build
```

The current workspace environment has the .NET runtime but not the .NET SDK, so compilation and launch have not been verified here.

## Internet service

Set `HEARTHLINE_API_BASE_URL` to the root URL of an HTTPS building service before starting the app. Do not put credentials in the URL or commit them to source control. The desktop client does not contain a shared staff secret; each connected user signs in and receives an individual bearer token.

The service contract expected by the app is:

- `GET /health` returns a successful HTTP status.
- `POST /api/auth/login` accepts `{"email":"...","password":"..."}` and returns `{"accessToken":"...","role":"admin|resident","resident":{"name":"...","apartment":"..."}}`. The `resident` object is required for resident accounts.
- `GET /api/building/snapshot` returns arrays named `residents`, `apartments`, `maintenance`, `guests`, `announcements`, and `events`. Their object fields match the corresponding C# properties in `Models.cs`.
- Authenticated `POST` requests create records at `/api/maintenance`, `/api/guest-passes`, `/api/announcements`, and `/api/events`.

The service must enforce authentication and role permissions on every protected endpoint. The app attempts writes only for authenticated sessions. If a write fails, it displays a warning; the item remains visible in the current app session and is not durable unless the service accepted it.

Without `HEARTHLINE_API_BASE_URL`, demo mode uses generated sample records. Demo edits exist only in memory and disappear when the app closes. The sample names and contact details are fictional.