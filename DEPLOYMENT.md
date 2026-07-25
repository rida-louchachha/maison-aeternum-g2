# Deploying Maison Aeternum to Railway

The app is container-ready: `Dockerfile` builds `g2soire/MaisonAeternum.Web.csproj`
into a self-contained ASP.NET Core 8 image, and `Program.cs` already handles the
two things Railway needs that local development doesn't:

- Binds to `$PORT` instead of the fixed `launchSettings.json` ports.
- Trusts `X-Forwarded-For` / `X-Forwarded-Proto` from Railway's edge proxy, so
  `CookieSecurePolicy.Always` and HSTS don't cause a redirect loop.

Database migrations and demo-data seeding run automatically on startup
(`DataSeeder.SeedAsync` → `Database.MigrateAsync()`), so no manual migration step
is needed after deploy.

## 1. Database

This project uses SQL Server (`UseSqlServer`), which is not one of Railway's
built-in managed databases (Railway's plugins are Postgres/MySQL/Mongo/Redis).
Pick one:

- **Deploy SQL Server as a second Railway service** — "New Service" → "Docker
  Image" → `mcr.microsoft.com/mssql/server:2022-latest`, with env vars
  `ACCEPT_EULA=Y` and `MSSQL_SA_PASSWORD=<strong password>`. Railway gives it an
  internal hostname you reference from the web service's connection string.
- **Use an external managed SQL Server** (Azure SQL Database free/serverless
  tier is the easiest match) and point the connection string at it.

Either way, the connection string just needs to reach a SQL Server instance —
nothing else in the app is Azure- or Railway-specific.

## 2. Required environment variables

Set these on the Railway **web** service (Settings → Variables). Railway auto-sets
`PORT`; you don't need to add it yourself.

| Variable | Purpose | Required? |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string (double underscore = ASP.NET Core config nesting for `ConnectionStrings:DefaultConnection`) | Yes |
| `Ai__HeyGen__ApiKey` | HeyGen avatar API key | Only if using HeyGen (leave unset to fall back to the mock avatar) |
| `Ai__HeyGen__VoiceId` | HeyGen voice id | Only if using HeyGen |
| `Ai__Anam__ApiKey` | Anam.ai API key | Only if switched to Anam (see `AiMentorServiceCollectionExtensions.cs`) |
| `Ai__Anam__AvatarId` / `Ai__Anam__VoiceId` | Anam.ai avatar/voice ids | Only if switched to Anam |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` | Already baked into the Dockerfile; override only if you need `Development` logging |

Without a HeyGen/Anam API key, `FallbackAvatarClient` automatically serves the
mock avatar, so the app runs fully functional without those keys — just without
a real talking-head video.

## 3. Deploy steps (manual, dashboard-driven)

1. Push this repo to GitHub (see below).
2. In Railway: **New Project → Deploy from GitHub repo**, pick the repo.
3. Railway detects the `Dockerfile` at the repo root and builds from it — no
   extra build configuration needed.
4. Add the SQL Server service (or external DB) from step 1, then set the
   environment variables from step 2 on the web service.
5. Deploy. On first boot the app creates the schema and seeds demo data
   (admin login: `admin@maisonaeternum.com` / `MaisonAdmin!2026` — **change
   this password after first login on a public deployment**).
6. Railway assigns a public `*.up.railway.app` domain automatically under
   Settings → Networking → Generate Domain.

## 4. Local Docker sanity check (optional, before pushing to Railway)

```bash
docker build -t maison-aeternum .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="<your SQL Server connection string>" \
  maison-aeternum
```
