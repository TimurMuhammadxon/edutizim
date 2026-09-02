# edutizim — client

React + TypeScript + Vite frontend for **edutizim**, a multi-tenant CRM/ERP/LMS platform for training centers (o'quv markazlar). Talks to the .NET backend in `../tizimServer/OnlineTesting`.

## Stack

- React 19, TypeScript, Vite
- react-router-dom v7, @tanstack/react-query v5, zustand v5
- react-hook-form + zod, axios, Radix UI + Tailwind (shadcn-style)
- Vitest for tests

## Getting started

```bash
npm install
npm run dev      # dev server, proxies /api to the backend on :5008
```

The backend (Postgres + MinIO via `docker-compose.yml`, then `dotnet run`) needs to be running for anything past the public landing/login pages — see `../tizimServer/OnlineTesting/CLAUDE.md`.

## Scripts

| Command | Does |
|---|---|
| `npm run dev` | Start the Vite dev server |
| `npm run build` | Type-check (`tsc -b`) then production build |
| `npm run lint` | ESLint, blocking in CI |
| `npm run test` | Vitest |

## Structure

```
src/
├── api/          # axios calls per domain (leads, students, groups, ...)
├── components/   # shared/ (Logo, CrudTable, dialogs, ...) and ui/ (shadcn primitives)
├── layouts/      # AppLayout (authenticated shell), AuthLayout, route guards
├── lib/          # i18n.ts, session.ts, jwt.ts, errors.ts, groupHelpers.ts
├── pages/        # route-level pages, grouped by area (crm/, admin/, public/, auth/)
├── store/        # zustand stores (auth, branch, language, theme)
└── types/        # shared DTO types
```

Three locales are supported (`uz-latn` default, `ru`, `uz-cyrl`) via `useTranslation()` in `src/lib/i18n.ts` — every CRM/admin page goes through it. See the backend `CLAUDE.md` for the fuller architecture writeup (multi-tenancy, auth, conventions) that covers both halves of the app.
