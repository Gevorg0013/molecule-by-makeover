# Molecule by Makeover — Frontend

React 19 + Vite + TypeScript app covering both the customer-facing storefront and the admin
panel for the [backend API](../backend/README.md).

## Stack

- React 19, React Router 7 (data router), Vite
- Tailwind CSS + hand-rolled shadcn/ui-style primitives (Radix UI + class-variance-authority)
- TanStack Query for all server state; Zustand for auth/UI client state
- react-hook-form + zod for forms
- react-i18next for UI chrome strings (EN/HY/RU); content strings (product names, categories,
  blog, pages) are resolved server-side via `?lang=`
- Stripe Elements for checkout payment confirmation

## Running locally

```bash
cd frontend
cp .env.example .env   # point VITE_API_BASE_URL at your running API
npm install
npm run dev
```

Requires the backend API running and reachable at `VITE_API_BASE_URL` (default
`http://localhost:5116/api/v1` for `dotnet run`, or `http://localhost:8080/api/v1` for the
Dockerized API). The backend's `Cors:AllowedOrigins` must include the Vite dev origin
(`http://localhost:5173` by default).

## Structure

```
src/
  api/          axios client + one module per backend resource (public + api/admin/*)
  types/        TS types mirroring backend DTOs/enums
  store/        Zustand stores (auth, UI)
  hooks/        TanStack Query hooks per resource (public + hooks/admin/*)
  i18n/         react-i18next config + en/hy/ru locale bundles
  components/   ui/ (primitives), storefront/, admin/, common/
  layouts/      StorefrontLayout, AuthLayout, AdminLayout
  pages/        storefront pages + pages/admin/* (lazy-loaded)
  routes/       router.tsx, ProtectedRoute, AdminRoute
  lib/          utils, currency/date formatting, query client, Stripe client
```

## Scripts

- `npm run dev` — Vite dev server
- `npm run build` — typecheck (`tsc -b`) + production build
- `npm run typecheck` — typecheck only
- `npm run preview` — preview the production build locally

## Notes

- Auth: access token lives in memory (Zustand), sent as `Authorization: Bearer`; the refresh
  token is an httpOnly cookie the app never reads directly — `credentials: 'include'` is set
  on every request.
- Admin routes (`/admin/*`) are code-split and gated behind both an authenticated session and
  the `Admin` role.
