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

This project uses PostgreSQL (`UseNpgsql`) — one of Railway's built-in managed
databases, so this is a couple of clicks:

- In the Railway project, **New → Database → Add PostgreSQL**. Railway
  provisions it and exposes connection variables automatically (host, port,
  user, password, database name — visible on that service's Variables tab).
- Reference it from the web service's `ConnectionStrings__DefaultConnection`
  (see below), built from those values.

> The project originally targeted SQL Server, but Railway has no built-in
> SQL Server plugin, and running Microsoft's SQL Server Docker image yourself
> needs ~2GB RAM — more than fits in Railway's free/hobby tier (confirmed by
> an actual OOM crash on that tier during setup). Postgres runs comfortably
> in that tier, and EF Core's provider abstraction meant swapping
> `Microsoft.EntityFrameworkCore.SqlServer` for
> `Npgsql.EntityFrameworkCore.PostgreSQL` only touched `DependencyInjection.cs`,
> the migrations, and connection strings — no repository/service code changed.

## 2. Required environment variables

Set these on the Railway **web** service (Settings → Variables). Railway auto-sets
`PORT`; you don't need to add it yourself.

| Variable | Purpose | Required? |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Npgsql connection string, e.g. `Host=<pg-host>;Port=5432;Database=<db>;Username=<user>;Password=<pass>` (double underscore = ASP.NET Core config nesting for `ConnectionStrings:DefaultConnection`) | Yes |
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
4. Add the Postgres database from step 1, then set the environment variables
   from step 2 on the web service.
5. Deploy. On first boot the app creates the schema and seeds demo data
   (admin login: `admin@maisonaeternum.com` / `MaisonAdmin!2026` — **change
   this password after first login on a public deployment**).
6. Railway assigns a public `*.up.railway.app` domain automatically under
   Settings → Networking → Generate Domain.

## 4. Local Docker sanity check (optional, before pushing to Railway)

```bash
docker build -t maison-aeternum .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=<pg-host>;Port=5432;Database=<db>;Username=<user>;Password=<pass>" \
  maison-aeternum
```
