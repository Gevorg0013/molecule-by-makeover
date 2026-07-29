# Molecule by Makeover

Multilingual (EN/HY/RU) premium hair & beard care e-commerce platform.

- **Backend**: ASP.NET Core 9, Clean Architecture, PostgreSQL — see [`backend/README.md`](backend/README.md).
- **Frontend**: React 19 + Vite + TypeScript, Tailwind + shadcn/ui, TanStack Query, Stripe
  checkout — storefront + admin panel — see [`frontend/README.md`](frontend/README.md).

## Quick start (full stack, via Docker)

```bash
cp .env.example .env   # set JWT_SIGNING_KEY and Stripe keys
docker compose up --build
```

Frontend: `http://localhost:5173` · API: `http://localhost:8080` · Swagger:
`http://localhost:8080/swagger` · Health: `http://localhost:8080/health`

Demo admin login (seeded): `admin@moleculebymakeover.com` / `Admin@12345` — change this
password immediately in any non-local environment.

See [`backend/README.md`](backend/README.md) for local (non-Docker) API setup, migrations,
seed data, and architecture notes. See [`frontend/README.md`](frontend/README.md) for local
(non-Docker) frontend setup.
