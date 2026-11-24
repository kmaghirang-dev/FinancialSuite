# FinancialSuite

A reusable ASP.NET Core 8 Web API starter. Copy this folder, rename it, configure it, and start building features on top of
infrastructure that already works: layered architecture, CQRS with MediatR, Identity + role/policy/resource authorization,
EF Core + SQL Server, parent-child dropdown lookups, error logging, utilities, and a full unit/integration test setup.

Coding conventions are in [CODING_GUIDELINES.md](CODING_GUIDELINES.md). Read it before adding features.

## What's inside

```
FinancialSuite/
├── FinancialSuite.slnx
├── global.json                         # opts dotnet test into Microsoft.Testing.Platform (xunit.v3)
├── CODING_GUIDELINES.md
├── src/
│   ├── FinancialSuite.Domain/         # entities, repository interfaces, constants, exceptions, utility interfaces
│   ├── FinancialSuite.Application/    # MediatR commands/queries, validators, DTOs, AutoMapper profiles, current user
│   ├── FinancialSuite.Infrastructure/ # DbContext, repositories, migrations, seeders, authorization, utilities
│   └── FinancialSuite/                # Web API (namespace FinancialSuite.API): controllers, middlewares, Program.cs
└── tests/
    ├── FinancialSuite.Application.Tests/  # handler/validator/DTO-profile unit tests (xunit.v3, Moq, FluentAssertions)
    └── FinancialSuite.API.Tests/          # controller + middleware tests via WebApplicationFactory<Program>
```

Dependency direction: `API → Application + Infrastructure`, `Infrastructure → Application → Domain`.

Request flow: `Controller → IMediator → Handler (+ FluentValidation, AutoMapper) → I{Entity}Repository → MyApplicationDbContext`.

## Reusable building blocks

| Area | Where | Notes |
| --- | --- | --- |
| Generic repository | `Domain/Repositories/IBaseRepository.cs`, `Infrastructure/Repositories/BaseRepository.cs` | GetAll / GetById / Create (returns key) / Delete / SaveChanges |
| Paging, search, sort | `Application/Common/PagedResult.cs`, `Domain/Constants/SortDirection.cs`, `GetAllMatchingAsync` in repositories | Column whitelist via `columnSelector` dictionary |
| **Dropdowns (parent-child)** | `DropDownCategory` (parent) → `DropDownDetail` (child) | Full CRUD + toggle + paging for both; lookup by category value |
| Authorization | `Infrastructure/Authorization/` | Roles, custom claims, policy requirement, resource-based service (see below) |
| Current user | `Application/Users/UserContext.cs`, `CurrentUser.cs` | `IUserContext.GetCurrentUser()` |
| User management | `Application/Users/Commands/*`, `IdentityController` | Update details, assign/unassign roles |
| Error handling | `API/Middlewares/ErrorHandlingMiddleware.cs` | `NotFoundException` → 404, `ForbidException` → 403, other → 500 + `ErrorLogs` row |
| Slow request logging | `API/Middlewares/RequestTimeLoggingMiddleware.cs` | Logs requests over 4 s |
| Logging | Serilog, configured from `appsettings.json` | |
| Validation | FluentValidation, auto-registered from the Application assembly | |
| Mapping | AutoMapper profiles in each feature's `Dtos/` folder | |
| HTTP client wrapper | `IApiConnector` / `Infrastructure/Utilities/Api/ApiConnector.cs` | Typed GET/POST/PUT/DELETE; named-client example in `AddInfrastructure` |
| Encryption | `IEncryptionService` / `Utilities/Encryption/EncryptionService.cs` | AES with a base64 32-byte key (`Encryption:Key`) |
| Excel reading | `IExcelReaderService` / `Utilities/Excel/` | .xlsx/.xls via NPOI plus old BIFF2–4 worksheets, header row → dictionaries |
| Word templates | `IWordDocumentService` / `Utilities/Word/` | Placeholder replacement + table row cloning (OpenXML) |
| Seeders | `Infrastructure/Seeders/*` | Dropdowns, roles, users, sample projects; run on startup in `Program.cs` |
| Swagger | `WebApplicationBuilderExtensions.AddPresentation` | Bearer auth wired up |

### Dropdown entities

- `DropDownCategory` (parent, e.g. `MediaType`) has many `DropDownDetail` children (`Code`, `DisplayValue`).
- Category endpoints (`SuperAdmin`): `api/dropdown/category` — GET all, GET `paginated`, GET `{id}` (includes details), POST, PATCH `{id}`, PATCH `toggle/{id}`, DELETE `{id}`.
- Detail endpoints (`SuperAdmin`): `api/dropdown/category/{id}/details` — GET all for category, GET `paginated`, GET `{detailId}`, POST, PATCH `{detailId}`, PATCH `toggle/{detailId}`, DELETE `{detailId}`, DELETE all for category.
- Lookup endpoint (any signed-in user): `GET api/dropdown/details/{categoryValue}` — what UI selects call.
- Category names used in code live in `Domain/Constants/DropDownCategoryValues.cs`; seed values in `Seeders/DropDownDetailsSeeder`.
- Other entities reference lookups by FK to `DropDownDetail` — see `MediaLibrary.CategoryId` / `StatusId` and their configuration in `MyApplicationDbContext.OnModelCreating`, plus category filtering in `GetAllPaginatedMediaLibraryQuery.CategoryId`.

### Authorization samples (kept intentionally)

- **Roles**: `UserRoles` (`User`, `Admin`, `SuperAdmin`), seeded by `RolesSeeder`; used as `[Authorize(Roles = ...)]`.
- **Custom claims**: `FinancialSuiteClaimsPrincipalFactory` adds `Nationality` and `DateOfBirth` claims (`AppClaimTypes`).
- **Policy + requirement**: `PolicyNames.AtLeast20` → `MinimumAgeRequirement` + `MinimumAgeRequirementHandler`; a claim-based policy (`HasNationality`) is shown commented out in `AddInfrastructure`.
- **Resource-based**: `IProjectAuthorizationService` / `ProjectAuthorizationService` — owner can update/delete, `SuperAdmin` can delete, anyone can read/create. Used by the `Projects` handlers, which throw `ForbidException` on failure.
- **Identity endpoints**: `MapIdentityApi<User>()` under `api/identity` (register, login, refresh, ...).

### Sample features

| Feature | Demonstrates | Safe to remove? |
| --- | --- | --- |
| `DropDownCategory`, `DropDownDetails` | Core lookup infrastructure | No — keep |
| `Users` + `IdentityController` | Identity user/role management | No — keep |
| `Projects` | CRUD + paging + resource-based authorization + owner FK to `User` | Yes, once you have your own example; also remove `ProjectsSeeder`, `IProjectAuthorizationService` usage, tests |
| `MediaLibrary` | CRUD + entity with two dropdown FKs + filtering by category | Yes; also remove `MediaType`/`MediaStatus` seeds and constants, `DropDownDetail.MediaLibrary*` navigations, tests |
| `ErrorTestController` | Throws to exercise `ErrorHandlingMiddleware` | Yes (keep its test only if you keep it) |

When removing a sample, delete its Domain entity/repository interface, Infrastructure repository + DI registration + `DbSet`/model config, Application feature folder, controller, and tests, then add a migration.

## Getting started

Prerequisites: .NET SDK 8+ (the solution targets `net8.0`; the .NET 10 SDK works too), SQL Server (LocalDB/Express/full), and the EF CLI (`dotnet tool install -g dotnet-ef`).

### 1. Configure the database

Edit `ConnectionStrings:DefaultConnection` in `src/FinancialSuite/appsettings.json` (default: `Server=localhost;Database=FinancialSuite;Trusted_Connection=True;TrustServerCertificate=True;`). For credentials, use user secrets instead of the file:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>" --project src/FinancialSuite
```

Create the schema from the included `InitialCreate` migration:

```bash
dotnet ef database update --project src/FinancialSuite.Infrastructure --startup-project src/FinancialSuite
```

Add migrations later with:

```bash
dotnet ef migrations add <Name> --project src/FinancialSuite.Infrastructure --startup-project src/FinancialSuite
```

### 2. Configure secrets

`Encryption:Key` (base64, 32 bytes) is required at startup. `appsettings.Development.json` contains a **development-only placeholder** so the app and tests run out of the box. For every real project generate a new key and keep it in user secrets / environment variables:

```powershell
$b = New-Object byte[] 32; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

```bash
dotnet user-secrets set "Encryption:Key" "<generated key>" --project src/FinancialSuite
```

### 3. Seed users

On startup the seeders create roles, dropdown values, sample projects, and three users when the `AspNetUsers` table is empty:
`user@example.com` / `user123`, `admin@example.com` / `admin123`, `superadmin@example.com` / `superadmin123`.
These are for local development only — change or remove them in `Seeders/UsersSeeder/UsersSeeder.cs` before deploying anywhere.

### 4. Configure authorization

- Add roles in `Domain/Constants/UserRoles.cs` (and `RolesSeeder`).
- Add claims in `FinancialSuiteClaimsPrincipalFactory`, names in `Authorization/Constants.cs`.
- Add policies in `AddInfrastructure` → `AddAuthorizationBuilder()`; register handlers as `IAuthorizationHandler`.
- For per-record rules, follow `ProjectAuthorizationService`.

### 5. Run

```bash
dotnet run --project src/FinancialSuite
```

Swagger: `https://localhost:7265/swagger` (profiles in `Properties/launchSettings.json`). Log in via `POST api/identity/login`, then use the `accessToken` as a Bearer token. Sample requests: `src/FinancialSuite/FinancialSuite.API.http`.

### 6. Run the tests

```bash
dotnet test --solution FinancialSuite.slnx
```

The API tests use `WebApplicationFactory<Program>` with repositories mocked, and `FakePolicyEvaluator` to bypass authentication, so they do not need a database.
Run one project with `dotnet test --project tests/FinancialSuite.Application.Tests`.

## Starting a new project from this template

1. Copy the whole `FinancialSuite` folder and delete any `bin/`, `obj/`, `.vs/` folders inside the copy.
2. From the copy's root, rename folders/files and replace the name in file contents (PowerShell, replace `MyNewApp`):

   ```powershell
   $new = "MyNewApp"
   Get-ChildItem -Recurse -Directory -Filter "*FinancialSuite*" | Sort-Object { $_.FullName.Length } -Descending | Rename-Item -NewName { $_.Name -replace "FinancialSuite", $new }
   Get-ChildItem -Recurse -File -Filter "*FinancialSuite*" | Rename-Item -NewName { $_.Name -replace "FinancialSuite", $new }
   Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.slnx,*.json,*.http,*.md,*.sql | ForEach-Object {
       $text = [IO.File]::ReadAllText($_.FullName)
       if ($text -match "FinancialSuite") { [IO.File]::WriteAllText($_.FullName, $text -replace "FinancialSuite", $new, (New-Object Text.UTF8Encoding $true)) }
   }
   ```

3. Give the API project a new `<UserSecretsId>` GUID in its `.csproj` (so projects don't share secrets) and generate a new `Encryption:Key`.
4. Update the connection string database name, then either keep the `InitialCreate` migration or delete `Migrations/` and run `dotnet ef migrations add InitialCreate`.
5. `git init`, build, run tests, and remove the samples you don't need (see table above).
