# tizim — multi-tenant CRM/ERP/LMS for training centers (учебные центры)

Rebuilt from an earlier single-tenant driving-license test-prep app (PravaDrive); the
.NET namespace (`OnlineTesting.*`) and solution/project names are kept as-is from that
project and have not been rebranded. Backend build order: CRM first, then ERP, then LMS.

## Stack
- ASP.NET Core Web API, .NET 8
- Clean Architecture: Domain / Application / Infrastructure / API
- MediatR (CQRS), FluentValidation via IPipelineBehavior
- EF Core + PostgreSQL (snake_case via EFCore.NamingConventions)
- BCrypt, JWT + refresh token rotation, Google OAuth login, Telegram WebApp login (HMAC)
- MinIO (S3-compatible) via AWSSDK.S3 for file storage
- xUnit + EF Core InMemory for backend tests (`OnlineTesting.Tests`)
- docker-compose (dev only): PostgreSQL on port 15432, MinIO on ports 9000/9001
- GitHub Actions CI: `.github/workflows/backend-ci.yml` (build + test), path-scoped to `tizimServer/**`

## Solution layout
```
src/
├── OnlineTesting.Domain/          (entities, base Entity, ITenantScoped)
├── OnlineTesting.Application/     (CQRS handlers, validators, behaviors, interfaces)
├── OnlineTesting.Infrastructure/  (EF Core, ApplicationDbContext, JWT, Storage)
├── OnlineTesting.API/             (Controllers, Middleware, Localization)
└── OnlineTesting.Tests/           (xUnit, EF Core InMemory)
```

## Multi-tenancy (the foundation everything else sits on)
- `Organization` is the tenant. `ITenantScoped` marker interface (just `Guid OrganizationId`)
  is implemented by every org-scoped entity (`Lead`, `Student`, `Group`, `Branch`, `Room`,
  `CrmTask`, `Attendance`, `Payment`, ...).
- `ApplicationDbContext.OnModelCreating` reflects over every entity type, and for any type
  implementing `ITenantScoped` attaches a global EF Core query filter comparing
  `OrganizationId` against `ICurrentUser.OrganizationId` (from the JWT's org claim).
  Platform roles (`Owner`/`SuperAdmin`, `OrganizationId == null`) bypass the filter; an
  unauthenticated/background context with no org and no platform role matches nothing —
  a safe default, not an accidental cross-tenant leak.
- **Known gap:** `GroupStudent` (the Group↔Student join/membership row) has no
  `OrganizationId` of its own, so the filter can't reach it directly — every handler that
  mutates a membership by `(GroupId, StudentId)` must first resolve the `Group` through the
  filtered `Groups` DbSet and 404 if it's not found, *before* touching `GroupStudents`. This
  was a real cross-tenant IDOR (fixed 2026-09-01) — when adding a new `GroupStudent`
  handler, follow the pattern in `AddStudentToGroupHandler`/`SetAttendanceHandler`, not a
  direct `GroupStudents.FirstOrDefaultAsync(...)`.
- Self-registration (`POST /auth/register`) creates an `Organization` + its `OrgAdmin` user
  atomically — there's no separate "create org" step.

## Roles
`Owner`(1)/`SuperAdmin`(2) = platform-level, cross-tenant, `OrganizationId` always null.
`OrgAdmin`(3)/`Teacher`(4)/`Student`(5)/`Staff`(6) = org-scoped, always has an `OrganizationId`.
`Teacher`/`Student` are reserved for the future LMS phase — not really in use yet in the CRM
phase (staff/teacher accounts are provisioned by an OrgAdmin via `POST /org/members/*`, not
by self-registration). Authorization policies (`Domain/Authorization/Roles.cs` +
`Program.cs`): `OwnerAccess`, `PlatformAccess`, `OrgAdminAccess`, `CrmAccess`, `GroupsAccess`
(least → most permissive is the reverse of that list; see the file for exact role sets).

## Conventions

**Domain**
- Private constructors, static factories, no setters — only behaviour methods
- Base `Entity` (non-generic): `Guid Id { get; protected set; }`
- Business rules that can be expressed without I/O live in Domain (e.g. `BalanceCalculator`,
  `GroupStudent`'s discount/freeze/effective-price logic) — handlers stay thin

**Application**
- Folder pattern: `{Domain}/{Feature}/Commands|Queries/{Action}/{Action}Command.cs + Handler.cs + Validator.cs`
- Infrastructure interfaces in `Common/Interfaces/`: `IApplicationDbContext`, `IJwtService`,
  `IPasswordHasher`, `IRequestContext`, `ICurrentUser`, `IDbExceptionInspector`,
  `ITelegramAuthValidator`, `IGoogleAuthValidator`, `ILanguageContext`, `IStorageService`
- Custom exceptions: `ValidationException`, `ConflictException`, `NotFoundException`, `UnauthorizedException`
- `ValidationBehavior` via `IPipelineBehavior`; validators/handlers are assembly-scanned
  (MediatR + FluentValidation), no manual DI registration needed per feature
- `PagedResult<T>` in `Common/Models/`

**Infrastructure**
- All entity configs via `IEntityTypeConfiguration<T>` in `Persistence/Configurations/`
- `OnModelCreating` uses `ApplyConfigurationsFromAssembly` — new configs picked up automatically
- `PostgresExceptionInspector.IsUniqueConstraintViolation(Exception)` for SQLSTATE 23505
- `MinioStorageService` / `FileSystemStorageService` behind `IStorageService` (local-path
  config picks the filesystem implementation, otherwise MinIO)

**API**
- `ExceptionHandlingMiddleware` maps custom exceptions to RFC 7807 `ProblemDetails`
- JWT: `MapInboundClaims = false`, `NameClaimType = "sub"`, `RoleClaimType = ClaimTypes.Role`,
  `Jwt:Key` required ≥32 chars from config/env (never hardcoded, not in any appsettings file)
- `JwtBearerEvents.OnAuthenticationFailed → NoResult()` (anonymous fallback for `[AllowAnonymous]`)
- `LanguageMiddleware` after Auth; reads `?lang=` (priority) or `Accept-Language`
- Rate limiting on auth endpoints (`auth-strict` for login/register, `auth-normal` for refresh/telegram/google)
- Swagger with Bearer security scheme (dev only)
- **Gap:** no global `AddAuthorization(options => options.FallbackPolicy = ...)` and some
  controllers (`GroupsController`, `BranchesController`, `RoomsController`, `MembersController`)
  have no class-level `[Authorize]`, relying entirely on correct per-action attributes — a new
  action added without one becomes silently anonymous. Add the attribute explicitly on every
  new action; don't rely on it being covered by a neighbor.

**Security**
- Constant-time defence on login (`CryptographicOperations.FixedTimeEquals` / dummy-hash BCrypt compare)
- Refresh token rotation with replay detection (SHA-256 hash storage, one-time-use,
  `RevokedAt`/`ReplacedByTokenHash` chaining, full-session revocation on reuse)
- Race-condition defence: UNIQUE indexes + catch `DbUpdateException` via `IDbExceptionInspector`
- Reserved domain `@telegram.local` blocked on registration

## Languages
Three: `uz-latn` (default), `ru`, `uz-cyrl`. Backend constants in
`Application/Common/Constants/Languages.cs`. Note: most CRM pages on the frontend
(Leads/Students/Groups/Finance/Rooms/Branches/Tasks) are hardcoded Uzbek strings, not wired
to the i18n system — only the older auth/subscription-era pages use it. Follow that pattern
(or fix it) deliberately, not by accident, when touching those pages.

## Modules (current, 2026-09-01)

**Auth** — `AuthController` (`/auth/register|login|refresh|telegram|google|logout`).
Entities: `User`, `RefreshToken`, `ExternalLogin`.

**CRM core**
- **Leads** (`LeadsController`, `/crm/leads`) — pipeline stage (`LeadStage`), manager
  assignment, convert-to-student.
- **Students** (`StudentsController`, `/crm/students`) — profile, attendance history,
  can get a login (`POST /crm/students/{id}/login`) to self-serve later.
- **Groups** (`GroupsController`, `/crm/groups`) — teacher/room assignment, schedule,
  membership (add/remove student, freeze/unfreeze/mark-left via `GroupStudent`), per-student
  discounts, attendance marking. This is the largest, most cross-cutting controller.
- **Attendance** (`Domain/Crm/Attendance.cs`, nested under Groups/Students endpoints).
- **Finance** (`FinanceController`, `/crm/finance`) — `Payment` (tuition payments, distinct
  from the old individual-subscription billing model, which was deleted — see below),
  debtor reports (`/debtors`, `/period-debts`, currently unpaginated — fine at current scale,
  will need pagination as an org's student count grows).
- **Tasks** (`TasksController`, `/crm/tasks`) — CRM follow-up reminders (`CrmTask`),
  complete/cancel/reschedule.
- **Dashboard** (`DashboardController`, `/crm/dashboard`) — summary stats.

**Organization structure**
- **Branches** (`BranchesController`, `/org/branches`), **Rooms** (`RoomsController`,
  `/org/rooms`, has a capacity check against `Group` membership count).
- **Members** (`MembersController`, `/org/members`) — OrgAdmin provisions Staff/Teacher
  accounts directly (no self-registration path for those roles).

**Admin** — `AdminUsersController` (`/admin/users`, Owner-only) — platform-wide user list.
(Filename note: this used to be misnamed `AdminSubscriptionsController.cs` from when it also
had a subscription-granting endpoint; renamed 2026-09-01 when that endpoint was removed.)

## Deleted (2026-09-01) — do not resurrect without a deliberate decision
The original PravaDrive individual-subscription billing model (Payme/Click payment gateway
integration, `SubscriptionPlan`/`Subscription`/`PaymentOrder`/`PaymeTransaction`/
`ClickTransaction`, the `/admin/plans` and `/admin/payments` admin pages, the `/subscription`
student page) was removed entirely. It was never migrated onto the `Organization` tenant
model, and its "buy a Teacher-type plan" flow silently auto-promoted `Student → Teacher`,
which was a real privilege-escalation bug. There is currently **no SaaS billing gate** for
organizations — that's a deliberate gap pending a real design, not an oversight. If billing
is needed again, design it against `Organization`, not against an individual `User`.

## Testing
`OnlineTesting.Tests` (xUnit) uses EF Core's InMemory provider to build a real
`ApplicationDbContext` (same `OnModelCreating`, same tenant query filter) rather than mocking
`IApplicationDbContext` — this is deliberate: the tenant filter is exactly the kind of logic
that's easy to accidentally bypass with a mock. `Common/TestDbContextFactory.cs` +
`Common/FakeCurrentUser.cs` are the seams for standing up an org-scoped context per test.
Current coverage: `GroupStudentTenantIsolationTests` (regression coverage for the IDOR fix
above). Coverage is intentionally narrow so far — extend it when touching risky logic
(financial calculations, auth, tenant boundaries), not as a blanket goal.

## Deployment
No production deployment exists yet for tizim itself — only local dev infra
(`docker-compose.yml`: Postgres 15432, MinIO 9000/9001). The original PravaDrive production
server (185.191.141.229, `/opt/pravadrive/`) is a separate, unrelated deployment for the old
single-tenant app — don't confuse the two or assume tizim's deployment steps mirror it.

## Working agreement
1. Architecture/discussion before code; no surprise refactors
2. After code, self-review with 🔴/🟡/🟢 priorities
3. After review, fixes by user approval
4. Migrations: show `Up()` for approval before `dotnet ef database update`
5. Build green is the gate between phases — `dotnet build` and `dotnet test` clean before moving on

## Common commands

Build:
```
dotnet build
```

Test:
```
dotnet test src/OnlineTesting.Tests/OnlineTesting.Tests.csproj
```

Add migration:
```
dotnet ef migrations add <Name> --project src/OnlineTesting.Infrastructure --startup-project src/OnlineTesting.API
```

Apply migration:
```
dotnet ef database update --project src/OnlineTesting.Infrastructure --startup-project src/OnlineTesting.API
```

Inspect DB:
```
docker exec -it tizim-pg psql -U postgres -d tizim -c "\d <table>"
```

## Style preferences (from previous sessions)
- Russian language for chat
- Compact explanations, no over-formatting
- Always state assumptions inline; ask only when truly blocked
