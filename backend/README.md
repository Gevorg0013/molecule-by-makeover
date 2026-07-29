# Molecule by Makeover — Backend API

ASP.NET Core 9 Web API for a multilingual (EN/HY/RU) premium hair & beard care e-commerce
platform. Clean Architecture, EF Core + PostgreSQL, JWT auth with refresh-token rotation,
FluentValidation, AutoMapper, Serilog, Swagger, and a provider-agnostic payment abstraction
(Stripe today; Idram/ArCa/Telcell can be added later without touching Application code).

## Solution layout

```
MoleculeByMakeover.slnx
src/
  Domain/          Entities, enums, value objects, repository interfaces - zero framework deps
  Shared/          Result<T>, PaginatedList<T>, RoleNames/LanguageCodes constants, exceptions
  Application/      Use-case services (Auth, Catalog, Ordering, Reviews, Content, Marketing,
                    Media, Admin), DTOs, FluentValidation validators, AutoMapper profile,
                    IPaymentProvider abstraction
  Infrastructure/   EF Core (AppDbContext, configurations, migrations, seed data), repositories
                    + UnitOfWork, JWT/password hashing, local file storage, Stripe adapter,
                    SMTP email, stock-reservation cleanup background job
  API/              Controllers, middleware (exception handling, localization), Program.cs
                    composition root, Swagger/JWT wiring
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL 16 (or `docker compose up postgres`)
- (Optional) Docker + Docker Compose for the full containerized stack

> This machine's default SDK was .NET 10; the project intentionally targets **net9.0** to
> match the required stack. If you only have a newer SDK installed, also install the .NET 9
> runtime (`dotnet-install.ps1 -Channel 9.0 -Runtime aspnetcore`) so `dotnet run`/`dotnet ef`
> can host net9.0 apps.

## Configuration

All settings live in `src/API/appsettings.json` (safe defaults for local dev) and can be
overridden via environment variables using the standard ASP.NET Core `Section__Key` convention
(see `docker-compose.yml` / `.env.example` at the repo root for the full list). Key sections:

| Section | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt` | Issuer/Audience/SigningKey, access-token (15 min) and refresh-token (14 day) lifetimes |
| `Payments:Stripe` | Stripe secret/publishable/webhook-signing keys |
| `FileStorage` | Local upload root + public base URL (swap `IFileStorageService` for S3/MinIO in production) |
| `Smtp` | Outbound email; leave `Host` empty to no-op emails in dev |
| `Cors:AllowedOrigins` | Frontend origin(s) allowed to call the API with credentials |
| `Frontend:BaseUrl` | Used to build Stripe return/cancel URLs at checkout |

**Never commit real secrets.** Rotate the `Jwt:SigningKey` and Stripe keys before deploying.

## Running locally

```bash
cd backend
dotnet restore
dotnet run --project src/API/MoleculeByMakeover.API.csproj
```

On first run, the API automatically:
1. Applies all EF Core migrations (`Database.MigrateAsync()`).
2. Seeds demo data (see below).
3. Loads the active-language cache used to resolve `?lang=hy|en|ru` without a DB hit per request.

Swagger UI is available at `https://localhost:<port>/swagger` in Development, with a "Bearer"
auth button pre-wired for pasting an access token.

## Running with Docker Compose

```bash
cp .env.example .env   # fill in JWT_SIGNING_KEY and Stripe keys at minimum
docker compose up --build
```

This starts PostgreSQL + the API (published `Release` build, non-root user, health-checked).

## Database migrations

```bash
cd backend
dotnet tool restore   # first time only - installs the pinned dotnet-ef 9.0.18 local tool
dotnet ef migrations add <Name> \
  --project src/Infrastructure/MoleculeByMakeover.Infrastructure.csproj \
  --startup-project src/API/MoleculeByMakeover.API.csproj \
  --output-dir Persistence/Migrations
```

## Demo / seed data

Seeded automatically on startup (idempotent - skipped if data already exists):

- **Languages**: `en` (default), `hy`, `ru`
- **Roles**: `Admin`, `Customer`
- **Admin login**: `admin@moleculebymakeover.com` / `Admin@12345` — **change this password
  immediately in any non-local environment.**
- 2 categories (Hair Care, Beard Care) and 2 products, each fully translated in all three
  languages, with tags, a demo coupon (`WELCOME10`, 10% off, AMD 10,000 minimum), the four
  static pages (about/privacy-policy/terms/faq), and baseline `Settings` rows.

## Authentication flow

1. `POST /api/v1/auth/register` or `/login` returns a short-lived JWT **access token** in the
   response body and sets an `HttpOnly`/`Secure`/`SameSite=Strict` **refresh-token** cookie
   scoped to `/api/v1/auth`.
2. On a `401`, the frontend calls `POST /api/v1/auth/refresh` (cookie sent automatically) to
   get a new access token; the refresh token is rotated with reuse detection - if a
   already-used refresh token is replayed, every active session for that user is revoked.
3. `POST /api/v1/auth/logout` revokes the current refresh token.

Admin-only endpoints live under `/api/v1/admin/**` and require the `Admin` role claim.

## Payment providers

Checkout and webhook handling depend only on `IPaymentProvider` / `IPaymentProviderResolver`
(`Application/Common/Interfaces/IPaymentProvider.cs`). `StripePaymentProvider` is the only
adapter registered today. To add Idram, ArCa, or Telcell:

1. Implement `IPaymentProvider` in `Infrastructure/Payments/`.
2. Register it: `services.AddScoped<IPaymentProvider, YourNewProvider>();` in
   `Infrastructure/DependencyInjection.cs`.
3. Nothing in `Application` or `API` changes - `PaymentProviderResolver` picks the right
   adapter by `ProviderKey` at checkout/webhook time.

## Notable design decisions

- **Repository + Unit of Work over EF Core** (not `DbContext` directly) so query/filter logic
  is centralized and testable, and Domain/Application never reference `Npgsql`.
- **Translation tables**, not per-language duplicate tables or JSON columns - see
  `ProductTranslation`, `CategoryTranslation`, `BlogPostTranslation`, `PageTranslation`,
  `BannerTranslation`. Each has a unique `(ParentId, LanguageId)` constraint and a
  `HasQueryFilter` matching its parent's soft-delete filter (EF Core will otherwise silently
  drop translations for a soft-deleted parent when eager-loading).
- **Stock is reserved at checkout**, not at payment confirmation, to prevent overselling; a
  background `StockReservationCleanupService` releases stock for orders whose payment never
  completes within 20 minutes.
- **Order line items snapshot** product name/SKU/price at purchase time, so later catalog
  edits (or a deleted product) never rewrite historical invoices.
