# Coding Guidelines — FinancialSuite

## 0. Scope Rules (Highest Priority — Mandatory for Every Task)

Read this whole document, starting with this section, before starting **every** task in this repository. These rules are mandatory, not recommendations. They take precedence over every other section. Where any later section describes a step as "optional", "acceptable", or "if the task warrants it" (for example adding sample requests to `FinancialSuite.API.http`, adding a new exception type, or adding seed data), do that step only when the owner explicitly requests it or it is strictly necessary for the requested change. Unlike the conventions in §1–§13, these rules are never overridden by what the existing code does.

### 0.1 Highest-priority working principle

The owner's explicit instructions define the scope of the task. Do not implement anything beyond that scope without the owner's approval, unless the additional change is strictly necessary to fulfill the request correctly.

### 0.2 Follow instructions exactly

- Implement only what is explicitly requested.
- Do not add extra features, enhancements, abstractions, interfaces, services, exceptions, or design patterns unless they are requested or strictly necessary to fulfill the request.
- Do not assume improvements beyond the requested scope are wanted.
- Do not turn a simple task into a larger refactoring or redesign.
- Suggest improvement opportunities separately; do not implement them without approval.

### 0.3 Never modify unrelated files

- Modify only files directly necessary to complete the request.
- Never delete, rename, overwrite, reorganize, or refactor unrelated files. This includes files or stubs the owner created that look empty or unused.
- Never change existing functionality outside the requested scope.
- Do not modify dependencies, configuration, database schemas, migrations, or other components unless explicitly requested or strictly necessary.
- Preserve existing code, working behavior, and project conventions wherever possible.

### 0.4 Ask before expanding scope

- If an instruction is ambiguous and the ambiguity materially affects the implementation, ask one concise clarifying question before implementing that part.
- If additional changes seem necessary, explain why and get approval before making them.
- Do not silently make assumptions that expand the scope of the request.
- Do not ask unnecessary questions when the request is already clear.

### 0.5 Respect the existing architecture

- Inspect the relevant existing code before implementing a task.
- Follow established project patterns instead of introducing new ones.
- Reuse existing classes, services, interfaces, utilities, and error-handling mechanisms when appropriate.
- Do not add an abstraction simply because it is considered a best practice.
- Prefer the simplest correct implementation that fits the existing architecture.

### 0.6 Testing and verification

- Build and run the relevant tests when appropriate to verify the change.
- Fix problems caused by your own change without making unrelated changes.
- Do not modify unrelated code just to resolve pre-existing issues.
- Clearly report pre-existing failures or blockers.

### 0.7 Final report (required for every task)

Briefly report:

- What was implemented.
- Which files were created, modified, or deleted, and why each change was necessary.
- Build and test results, when applicable.
- Any deviation from the explicit instructions, and why it was necessary.
- Optional improvements recommended but not implemented.

## 1. Purpose and General Principles

This document is the primary reference for implementing features, fixing bugs, and modifying code in this repository. Every rule below was derived from the code that exists today; file paths are given as evidence.

- **The existing code is the source of truth.** When this document and the code disagree, inspect the code, follow the dominant pattern in the area you are touching, and treat this document as outdated.
- **Make minimal changes.** Change only what the task requires. Do not refactor, rename, reformat, or "modernize" code you were not asked to touch.
- **Preserve architecture and behavior.** Keep the four-project layering, MediatR request/handler flow, AutoMapper profiles, FluentValidation validators, and repository abstraction exactly as they are.
- **Look before you write.** Before adding anything, find the closest existing implementation (usually in `DropDownCategory` or `DropDownDetails`) and mirror it.
- **Prefer consistency over novelty.** Do not introduce a new library, pattern, or abstraction when an existing one covers the need.
- **Do not copy known defects.** Section 13 lists implementation details that exist in the code but are bugs or questionable practices. Do not replicate them in new code, and do not fix them unless the task asks for it.

## 2. Repository Structure

### 2.1 Solution and projects

Solution file: `FinancialSuite.slnx`. All projects target `net8.0` with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.

| Project (folder) | Assembly / root namespace | Responsibility | References |
|---|---|---|---|
| `FinancialSuite.Domain/` | `FinancialSuite.Domain` | Entities, repository interfaces, domain exceptions | NuGet: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| `FinancialSuite.Application/` | `FinancialSuite.Application` | MediatR commands/queries/handlers, FluentValidation validators, AutoMapper profiles, DTOs, current-user context | Domain; NuGet: AutoMapper, MediatR, FluentValidation.DependencyInjectionExtensions, SharpGrip.FluentValidation.AutoValidation.Mvc |
| `FinancialSuite.Infrastructure/` | `FinancialSuite.Infrastructure` | EF Core `DbContext`, repository implementations, migrations, seeders, Identity store registration | Application; NuGet: EF Core SqlServer/Tools |
| `FinancialSuite/` (`FinancialSuite.API.csproj`) | `FinancialSuite.API` | ASP.NET Core Web API: controllers, middlewares, `Program.cs`, Swagger, Serilog | Application, Infrastructure |

Note that the API project **folder** is `FinancialSuite/` but its namespace is `FinancialSuite.API` (e.g. `FinancialSuite/Controllers/DropDownCategoryController.cs` declares `namespace FinancialSuite.API.Controllers`).

There are **no test projects** in the solution. There is no UI/client project; this is an API only.

### 2.2 Where things live

| Kind of file | Location | Example |
|---|---|---|
| Entity | `FinancialSuite.Domain/Entities/` | `DropDownCategory.cs` |
| Repository interface | `FinancialSuite.Domain/Repositories/` | `IDropDownCategoryRepository.cs` |
| Domain exception | `FinancialSuite.Domain/Exceptions/` | `NotFoundException.cs` |
| Command + handler + validator | `FinancialSuite.Application/<Feature>/Commands/<Action>/` | `DropDownCategory/Commands/Create/` |
| Query + handler | `FinancialSuite.Application/<Feature>/Queries/<Action>/` | `DropDownCategory/Queries/GetAll/` |
| DTOs and AutoMapper profile | `FinancialSuite.Application/<Feature>/Dtos/` | `DropDownCategoryProfile.cs` |
| Application DI registration | `FinancialSuite.Application/Extensions/ServiceCollectionExtension.cs` | `AddApplication()` |
| Current user | `FinancialSuite.Application/Users/` | `UserContext.cs`, `CurrentUser.cs` |
| DbContext | `FinancialSuite.Infrastructure/Persistence/MyApplicationDbContext.cs` | |
| Repository implementation | `FinancialSuite.Infrastructure/Repositories/` | `DropDownCategoryRepository.cs` |
| Seeder (interface + class) | `FinancialSuite.Infrastructure/Seeders/<Name>/` | `Seeders/DropDownDetailsSeeder/` |
| EF migrations | `FinancialSuite.Infrastructure/Migrations/` | generated by EF tooling |
| Infrastructure DI registration | `FinancialSuite.Infrastructure/Extensions/ServiceCollectionExtensions.cs` | `AddInfrastructure(...)` |
| Controller | `FinancialSuite/Controllers/` | `DropDownCategoryController.cs` |
| API/presentation DI registration | `FinancialSuite/Controllers/Extensions/WebApplicationBuilderExtensions.cs` | `AddPresentation()` |
| Middleware | `FinancialSuite/Middlewares/` | `ErrorHandlingMiddleware.cs` |
| Manual HTTP requests | `FinancialSuite/FinancialSuite.API.http` | |

### 2.3 Feature folder template

`FinancialSuite.Application/FinancialSuite.Application.csproj` declares an empty `FolderTemplate` scaffold, which is the intended layout for a new feature:

```
FolderTemplate/
  Commands/Create/  Commands/Delete/  Commands/Toggle/  Commands/Update/
  Dtos/
  Queries/GetAll/   Queries/GetById/
```

`DropDownCategory/` follows this template exactly. `DropDownDetails/` adds feature-specific action folders (`DeleteAllByCategory`, `DeleteById`, `GetAllByCategoryId`) and also contains empty placeholder folders (`Commands/DeleteAll`, `Commands/Update`, `Queries/GetAll`). Use the template's folder names for standard CRUD actions and a descriptive name (`DeleteById`, `GetAllByCategoryId`) only when the action is not one of the standard ones. Do not delete or edit `FolderTemplate` or the empty placeholder folders.

### 2.4 Architectural boundaries

- Controllers depend only on `IMediator` and Application request types. They never touch repositories or `MyApplicationDbContext`.
- Handlers depend on Domain repository interfaces (`IDropDownCategoryRepository`, `IDropDownDetailsRepository`), `IMapper`, and `ILogger<T>`. They never reference `MyApplicationDbContext` or Infrastructure types.
- Repository interfaces live in Domain; implementations live in Infrastructure and are registered in `AddInfrastructure`.
- The one exception to "handlers only use repositories": `FinancialSuite.Application/Users/Commands/UpdateUserDetailsCommandHandler.cs` uses ASP.NET Identity's `IUserStore<User>` directly. Follow that approach for `User` (Identity) data only.

## 3. Naming and File Organization

### 3.1 One type per file, file name = type name

Every file contains a single type named after the file. The only exception is `FinancialSuite.Application/Users/UserContext.cs`, which declares both `IUserContext` and `UserContext`. Do not generalize that exception; put new interfaces in their own file (as in `FinancialSuite.Domain/Repositories/` and `FinancialSuite.Infrastructure/Seeders/DropDownDetailsSeeder/IDropDownDetailsSeeder.cs`).

### 3.2 Namespaces

Namespaces mirror the folder path exactly, using the project's root namespace:

- `FinancialSuite.Application.DropDownCategory.Commands.Create`
- `FinancialSuite.Infrastructure.Seeders.DropDownDetailsSeeder`
- `FinancialSuite.API.Middlewares`

### 3.3 Feature names vs. entity names (important)

Application feature namespaces are named after entities (`FinancialSuite.Application.DropDownCategory`, `FinancialSuite.Application.DropDownDetails`). Inside those namespaces the bare identifier `DropDownCategory` resolves to the **namespace**, not the entity. The codebase therefore refers to entities with the qualified name `Domain.Entities.<Entity>`:

```csharp
// FinancialSuite.Application/DropDownCategory/Commands/Create/CreateDropDownCategoryCommandHandler.cs
var dropDownCategory = mapper.Map<Domain.Entities.DropDownCategory>(request);
```

Do the same in any new feature folder named after an entity. (`nameof(DropDownCategory)` in `NotFoundException` calls also resolves to the namespace; the resulting string is the same, so this is harmless.)

### 3.4 Type naming

| Kind | Pattern | Examples |
|---|---|---|
| Command | `{Action}{Feature}Command` | `CreateDropDownCategoryCommand`, `ToggleDropDownDetailsCommand`, `UpdateUserDetailsCommand` |
| Command handler | `{CommandName}Handler` | `CreateDropDownCategoryCommandHandler` |
| Validator | `{CommandName}Validator` | `UpdateDropDownCategoryCommandValidator` |
| Query | `{Action}{Feature}Query` | `GetAllDropDownCategoryQuery`, `GetByIdDropDownDetailsQuery` |
| Query handler | `{QueryName}Handler` | `GetAllDropDownCategoryQueryHandler`, `GetByIdDropDownDetailsQueryHandler` |
| DTO | `{Action}{Feature}Dto` (shape tied to the query that returns it) | `GetAllDropDownCategoryDto`, `GetByIdDropDownCategoryDto` |
| AutoMapper profile | `{Feature}Profile`, in the feature's `Dtos/` folder | `DropDownCategoryProfile`, `DropDownDetailsProfile` |
| Repository interface / impl | `I{Entity}Repository` / `{Entity}Repository` | `IDropDownDetailsRepository` / `DropDownDetailsRepository` |
| Controller | `{Feature}Controller` | `DropDownCategoryController`, `IdentityController` |
| Middleware | `{Purpose}Middleware` | `ErrorHandlingMiddleware`, `RequestTimeLoggingMiddleware` |
| DI extension class | `ServiceCollectionExtension(s)` / `WebApplicationBuilderExtensions` | see §4.6 |

Known deviations (do not copy): `GetByIdDropDownCategoryHandler` omits `Query`; `GetDropDownDetailsByCategoryIdQuery` puts the qualifier after the feature (`GetAllByCategoryId` folder); `DeleteAllDropDownDetailsByCategoryCommand`. When adding a variant action, prefer `{Action}{Feature}{Qualifier}Command/Query` consistent with the nearest sibling.

### 3.5 Members

- Properties, methods, classes: PascalCase.
- Primary-constructor parameters (the DI dependencies), locals, parameters: camelCase (`mediator`, `logger`, `mapper`, `repository`, `categoryRepository`, `detailsRepository`, `dbContext`).
- No private fields with `_` prefix exist — dependencies are used directly through primary-constructor parameters.
- Entity primary keys: `{Entity}Id` of type `Guid` (`DropDownCategoryId`, `DropDownDetailId`, `ProjectId`). `MediaLibrary` uses `MediaId` — an outlier.
- Repository methods use the `Async` suffix (`GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `DeleteAsync`) **except** `SaveChanges()`. Keep `SaveChanges()` as named when calling it; give new async repository methods the `Async` suffix.
- Controller actions do **not** use the `Async` suffix (`GetAll`, `GetById`, `Create`, `Update`, `Toggle`, `Delete`).
- Request-carried IDs are named after the entity key when set from the route (`DropDownCategoryId`, `DropDownDetailId`) and `Id`/`DetailId`/`CategoryId` when passed via constructor.

## 4. C# Coding Conventions

### 4.1 Formatting

- 4-space indentation, Allman braces (opening brace on its own line).
- **Block-scoped namespaces** (`namespace X { ... }`) in all files except `IDropDownDetailsSeeder.cs`, which is file-scoped. Use block-scoped for new files.
- Files use CRLF line endings; most `.cs` files are UTF-8 with BOM (Visual Studio defaults). Keep the encoding of files you edit.
- Short guard clauses are written on one line: `if (res is null) return NotFound();`, `if (data == null) throw new NotFoundException(...);`.
- Long primary-constructor parameter lists are wrapped one per line (see `DeleteByIdDropDownDetailsCommandHandler.cs`); short ones stay on one line (see `GetAllDropDownCategoryQueryHandler.cs`). Either is acceptable.
- Expression-bodied members are used for one-line delegations: `public async Task SaveChanges() => await dbContext.SaveChangesAsync();` and `public bool IsInRole(string role) => Roles.Contains(role);`.

### 4.2 Using directives

- `ImplicitUsings` is enabled, so `System`, `System.Linq`, `System.Collections.Generic`, `System.Threading.Tasks` are already available.
- Many files nevertheless carry the Visual Studio template block (`using System; using System.Collections.Generic; using System.Linq; using System.Text; using System.Threading.Tasks;`). **Do not add this block to new files** — the newer, cleaner files (`CreateDropDownCategoryCommand.cs`, `GetAllDropDownCategoryQueryHandler.cs`, controllers) omit it. **Do not strip it from existing files** you are not otherwise changing.
- Using order is not enforced (some files put `System.*` first, others last). Add new usings next to related ones without reordering the block.
- Remove usings you introduced and no longer need. Do not mass-clean unrelated unused usings.

### 4.3 Classes and construction

- **Primary constructors are the DI mechanism everywhere**: controllers, handlers, repositories, middlewares, seeders, `UserContext`, `MyApplicationDbContext`.
  ```csharp
  // FinancialSuite/Controllers/DropDownCategoryController.cs
  public class DropDownCategoryController(IMediator mediator) : ControllerBase
  ```
- Requests with only IDs use primary-constructor parameters assigned to properties:
  ```csharp
  // FinancialSuite.Application/DropDownCategory/Commands/Delete/DeleteDropDownCategoryCommand.cs
  public class DeleteDropDownCategoryCommand(Guid id) : IRequest
  {
      public Guid Id { get; } = id;
  }
  ```
  Requests bound from a JSON body use a parameterless class with `{ get; set; }` properties.
- Requests, DTOs, entities are **classes** with mutable auto-properties. The only `record` is `CurrentUser`. Do not convert existing types to records.
- Non-nullable `string` properties are initialized with `= string.Empty`; collections with `= new List<T>()` (`GetByIdDropDownCategoryDto`).
- Handler visibility is inconsistent: `DropDownCategory` handlers are `public`; `DropDownDetails` and `Users` handlers are `internal`. MediatR registers both. Match the visibility used in the feature folder you are working in. Validators must be `public` (see §13).

### 4.4 Async, cancellation, LINQ

- All I/O is `async`/`await` returning `Task`/`Task<T>`. No `.Result`/`.Wait()`.
- Handlers receive a `CancellationToken` but do **not** pass it to repositories — repository interfaces have no token parameters. The only token pass-through is to `IUserStore<User>` in `UpdateUserDetailsCommandHandler.cs`. Do not add token parameters to existing repository interfaces unless the task requires it.
- LINQ method syntax only (no query syntax). EF queries use `ToListAsync`, `FirstOrDefaultAsync`, `FindAsync`, `Include`.
- `IsActive` filtering is done **in the handler, in memory**, after the repository returns data:
  ```csharp
  // GetAllDropDownCategoryQueryHandler.cs
  var data = await repository.GetAllAsync();
  data = data.Where(x => x.IsActive);
  ```

### 4.5 Nullability

- Nullable reference types are enabled in all projects. Repository "find" methods return `T?` (`Task<DropDownCategory?> GetByIdAsync(Guid id)`).
- Navigation properties are nullable (`List<DropDownDetails>? DropDownDetails`), so access them with `?.` and handle `null` (`data.DropDownDetails ?? Enumerable.Empty<...>()` in `DeleteAllDropDownDetailsByCategoryCommandHandler.cs`).
- The null-forgiving operator `!` is used in places (`GetByIdDropDownCategoryHandler.cs`, `UserContext.cs`). Do not add new `!` to skip a not-found check — throw `NotFoundException` instead (§9.3).

### 4.6 Dependency injection

Each layer exposes one extension method, called from `FinancialSuite/Program.cs`:

```csharp
builder.AddPresentation();                                  // API
builder.Services.AddApplication();                          // Application
builder.Services.AddInfrastructure(builder.Configuration);  // Infrastructure
```

- `AddApplication()` (`FinancialSuite.Application/Extensions/ServiceCollectionExtension.cs`) scans the Application assembly for MediatR handlers, AutoMapper profiles, and FluentValidation validators. **New handlers, profiles, and public validators need no manual registration.** Explicit registrations (e.g. `IUserContext`) go here.
- `AddInfrastructure()` (`FinancialSuite.Infrastructure/Extensions/ServiceCollectionExtensions.cs`) registers the DbContext, Identity, seeders, and repositories as `AddScoped<IInterface, Implementation>()` under the existing `// Seeders` and `// Repository registrations` comments. Add new repositories there.
- `AddPresentation()` (`FinancialSuite/Controllers/Extensions/WebApplicationBuilderExtensions.cs`) registers controllers, authentication, Swagger, middlewares (`AddScoped<...Middleware>()`), and Serilog.

### 4.7 Comments

The hand-written code has essentially no comments, no XML documentation, and no TODOs (only EF-generated `/// <inheritdoc />` in migrations and short section comments in DI/`Program.cs`). Do not add XML docs or explanatory comments to code that follows existing patterns. A short comment is acceptable only for genuinely non-obvious logic.

## 5. Architecture and Application Layers

The application is a layered (Clean-Architecture-style) Web API using **CQRS via MediatR**. Request flow for every feature endpoint:

```
Controller action  →  mediator.Send(command/query)
  →  [FluentValidation auto-validation runs at model binding, before the action]
  →  IRequestHandler.Handle
       →  repository (Domain interface, Infrastructure implementation, EF Core)
       →  IMapper (entity ↔ request/DTO)
  →  controller wraps the result (Ok / CreatedAtAction / NoContent / NotFound)
Exceptions → ErrorHandlingMiddleware → 404 / 500
```

### 5.1 Command handler shapes

**Create** — map request → entity, persist via repository, return the new id:
```csharp
// CreateDropDownCategoryCommandHandler.cs
var dropDownCategory = mapper.Map<Domain.Entities.DropDownCategory>(request);
var id = await repository.CreateAsync(dropDownCategory);
return id;
```

**Update / Toggle** — load tracked entity, throw if missing, map the request onto it, then `SaveChanges()`:
```csharp
// UpdateDropDownCategoryCommandHandler.cs
var id = request.DropDownCategoryId;
var data = await repository.GetByIdAsync(id) ??
           throw new NotFoundException(nameof(DropDownCategory), id.ToString());

mapper.Map(request, data);

await repository.SaveChanges();
```

**Delete** — load, throw if missing, call `repository.DeleteAsync(entity)` (which saves internally).

**Child entities (`DropDownDetails`)** are loaded through the parent: handlers call `categoryRepository.GetByIdAsync(categoryId)` (which `Include`s details) and then search `data.DropDownDetails` in memory (see `DeleteByIdDropDownDetailsCommandHandler.cs`, `GetByIdDropDownDetailsQueryHandler.cs`). Writes of new children go through `detailsRepository.CreateAsync`. Follow this parent-first approach for nested resources.

### 5.2 Query handler shape

Load via repository → filter `IsActive` in memory → `mapper.Map<TDto>(...)` → return. See `GetAllDropDownCategoryQueryHandler.cs` and `GetDropDownDetailsByCategoryIdQueryHandler.cs`.

### 5.3 Where new functionality goes

For a new entity/feature (use `MediaLibrary` or `Projects` as the reference implementation):

1. Entity in `FinancialSuite.Domain/Entities/` (check if it already exists — several do).
2. `DbSet` + relationship config in `MyApplicationDbContext.cs`; EF migration in `FinancialSuite.Infrastructure/Migrations/` (only if the task requires schema changes).
3. `I{Entity}Repository` in `FinancialSuite.Domain/Repositories/`; `{Entity}Repository` in `FinancialSuite.Infrastructure/Repositories/`; register in `AddInfrastructure`.
4. Feature folder in `FinancialSuite.Application/{Feature}/` following the `FolderTemplate` layout; profile in `Dtos/`.
5. `{Feature}Controller` in `FinancialSuite/Controllers/`.
6. Optionally add sample requests to `FinancialSuite/FinancialSuite.API.http`.

Do not add a "service" layer, generic repository, unit of work, or result wrapper — none exist.

## 6. Backend and API Conventions

### 6.1 Controllers

Template: `FinancialSuite/Controllers/DropDownCategoryController.cs`.

- Attributes: `[ApiController]`, `[Route("api/...")]`, and `[Authorize]` (class-level on `DropDownCategoryController`, action-level on `IdentityController`). Inherit `ControllerBase`.
- Routes are lowercase, slash-separated resource paths: `api/dropdown/category`, `api/dropdown/category/{id}/details`, `api/identity`. Nested resources include the parent id in the controller route.
- Every action: `public async Task<IActionResult> ActionName(...)`, body is `mediator.Send(...)` plus a result wrapper. No business logic in controllers.
- Route IDs are copied into the body command before sending:
  ```csharp
  [HttpPatch("{id}")]
  public async Task<IActionResult> Update([FromRoute]Guid id, UpdateDropDownCategoryCommand command)
  {
      command.DropDownCategoryId = id;
      await mediator.Send(command);
      return CreatedAtAction(nameof(GetById), new { id = command.DropDownCategoryId }, null);
  }
  ```
  ID-only requests are constructed inline: `mediator.Send(new DeleteDropDownCategoryCommand(id))`.
- `[FromRoute]` is used in `DropDownCategoryController` but omitted in `DropDownDetailsController`; both work. Match the controller you are editing.

HTTP verb and response conventions in use:

| Operation | Verb / route | Response |
|---|---|---|
| Get all | `GET ""` | `Ok(result)` |
| Get by id | `GET "{id}"` | `Ok(result)` (category controller also returns `NotFound()` on `null`) |
| Create | `POST ""` | `CreatedAtAction(nameof(GetById), new { id }, null)` |
| Update | `PATCH "{id}"` | `CreatedAtAction(nameof(GetById), ...)` in `DropDownCategoryController` (existing contract) |
| Toggle active | `PATCH "toggle/{id}"` | `NoContent()` |
| Delete | `DELETE "{id}"` | `NoContent()` |
| Identity user update | `PATCH "user"` | `NoContent()` |

Updates use `PATCH`, not `PUT`. Soft enable/disable is a separate `toggle` endpoint that sets `IsActive` and `UpdatedBy`.

### 6.2 Requests and DTOs

- The command/query class **is** the request body model; there are no separate request DTOs.
- Response DTOs live in the feature `Dtos/` folder and are returned only from queries. Commands return `Guid` (create) or nothing (`IRequest`).
- `GetAll*Dto` exposes audit fields with dates as **strings formatted `"MM-dd-yyyy"`** (mapped in the profile). `GetById*Dto` types are slimmer and may nest child DTOs (`GetByIdDropDownCategoryDto.DropDownDetails` is `List<GetAllDropDownDetailsDto>`).

### 6.3 Mapping (AutoMapper)

All mapping is in `{Feature}Profile : Profile` constructors (`DropDownCategoryProfile.cs`, `DropDownDetailsProfile.cs`). No manual mapping code in handlers except `UpdateUserDetailsCommandHandler.cs` (Identity user).

Established mapping rules:
- Create command → entity: set `DateCreated` and `DateUpdated` to `DateTime.Now`, `IsActive` to `true`.
- Update / Toggle command → entity: set `DateUpdated` to `DateTime.Now`.
- Entity → `GetAll*Dto`: format `DateCreated`/`DateUpdated` with `ToString("MM-dd-yyyy")`.

```csharp
// DropDownCategoryProfile.cs
CreateMap<CreateDropDownCategoryCommand, Domain.Entities.DropDownCategory>()
    .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
    .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now))
    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
```

Use `DateTime.Now` (not `UtcNow`) for consistency with existing data unless the task says otherwise.

### 6.4 Audit fields

`CreatedBy` / `UpdatedBy` are **supplied by the client in the request body** (see `CreateDropDownCategoryCommand`, `ToggleDropDownCategoryCommand`, and the JSON in `FinancialSuite.API.http`); they are not derived from `IUserContext`. Keep this behavior for the dropdown features. `IUserContext` is currently used only by `UpdateUserDetailsCommandHandler`.

### 6.5 Authentication and authorization

- ASP.NET Core Identity with `AddIdentityApiEndpoints<User>()` + `AddEntityFrameworkStores<MyApplicationDbContext>()` (`FinancialSuite.Infrastructure/Extensions/ServiceCollectionExtensions.cs`), mapped in `Program.cs` via `app.MapGroup("api/identity").WithTags("Identity").MapIdentityApi<User>()`. Register/login are the built-in Identity endpoints.
- `User` (`FinancialSuite.Domain/Entities/User.cs`) extends `IdentityUser` with `DateOfBirth` and `Nationality`.
- Protect new controllers with `[Authorize]` as `DropDownCategoryController` does. Swagger is configured with a `bearerAuth` scheme in `WebApplicationBuilderExtensions.cs`.
- Get the current user through `IUserContext.GetCurrentUser()` (`FinancialSuite.Application/Users/UserContext.cs`); never read `HttpContext` in handlers directly.

### 6.6 External integrations

There are no outbound HTTP clients or external service integrations. Do not introduce one unless required.

## 7. Data Access and Persistence

### 7.1 EF Core and the DbContext

- SQL Server via `UseSqlServer(connectionString)`; connection string `DefaultConnection` in `FinancialSuite/appsettings.json`.
- `MyApplicationDbContext` derives from `IdentityDbContext<User>` and uses a primary constructor.
- `DbSet`s are declared **`internal`** with plural names (`DropDownCategories`, `ErrorLogs`) — except `DropDownDetails`, which is already plural-sounding. Add new `DbSet`s the same way.
- Keys use the `[Key]` data annotation on the entity. Relationships are configured with the fluent API in `OnModelCreating`, after `base.OnModelCreating(modelBuilder)`, always with `.OnDelete(DeleteBehavior.NoAction)`:
  ```csharp
  modelBuilder.Entity<DropDownDetails>()
      .HasOne(x => x.DropDownCategory)
      .WithMany(x => x.DropDownDetails)
      .HasForeignKey(x => x.DropDownCategoryId)
      .OnDelete(DeleteBehavior.NoAction);
  ```
- Check the model snapshot (`Migrations/MyApplicationDbContextModelSnapshot.cs`) before assuming a table exists.

### 7.2 Entities

Every business entity (`DropDownCategory`, `DropDownDetails`, `MediaLibrary`, `Project`) carries the same audit/soft-active block:

```csharp
public string CreatedBy { get; set; } = string.Empty;
public DateTime DateCreated { get; set; }
public string UpdatedBy { get; set; } = string.Empty;
public DateTime DateUpdated { get; set; }
public bool IsActive { get; set; }
```

Include it on new entities. Navigation properties are nullable (`DropDownCategory?`, `List<...>?`). Lookup values used by other entities (category/status) are foreign keys to `DropDownDetails` (see `MediaLibrary.CategoryId`/`StatusId`).

### 7.3 Repositories

Template: `FinancialSuite.Infrastructure/Repositories/DropDownCategoryRepository.cs`.

- One repository per aggregate, interface in Domain, implementation takes `MyApplicationDbContext dbContext` via primary constructor.
- Standard members: `GetAllAsync()`, `GetByIdAsync(Guid id)`, `CreateAsync(entity)` returning the new `Guid`, `DeleteAsync(entity)`, `SaveChanges()`.
- **`CreateAsync` and `DeleteAsync` call `SaveChangesAsync()` themselves.** Updates are done by mutating the tracked entity in the handler and calling `repository.SaveChanges()`. There is no unit-of-work or explicit transaction anywhere; each repository call is its own save.
- `GetByIdAsync` on a parent `Include`s its children (`DropDownCategoryRepository.GetByIdAsync`). Simple lookups use `FindAsync` (`DropDownDetailsRepository.GetByIdAsync`).
- Implementation visibility is inconsistent (`DropDownCategoryRepository` is `internal`, `DropDownDetailsRepository` is `public`). `internal` matches the other Infrastructure classes (`DropDownDetailsSeeder`) and the `internal` DbSets; prefer it for new repositories.

### 7.4 Migrations

- EF Core code-first migrations in `FinancialSuite.Infrastructure/Migrations/`, generated by EF tooling; the migrations assembly is Infrastructure and the startup project is the API.
- Never hand-edit generated migrations or the model snapshot. Add a migration only when the task requires a schema change, and review the generated `Up`/`Down` for unintended operations — e.g. `20261016174159_users-new-fields.cs` also dropped the `MiniPirateClash*` tables.
- Migration names are not consistent (`InitialMigration`, `IdentityAdded`, `users-new-fields`); use a short descriptive name.
- No raw SQL, stored procedures, or SQL scripts exist in the repository.

### 7.5 Seeding

`IDropDownDetailsSeeder.Seed()` runs at startup inside a scope in `Program.cs`. It only seeds when the database is reachable and `DropDownDetails` is empty. Seed values use `CreatedBy = "Seeder"`. Add new seed data to a seeder following `Seeders/DropDownDetailsSeeder/` only when the task requires it.

## 8. UI and Client-Side Conventions

Not applicable — the repository contains no UI or client-side project. Swagger UI (development only) and `FinancialSuite/FinancialSuite.API.http` are the only ways the API is exercised.

## 9. Configuration, Validation, Error Handling, and Logging

### 9.1 Configuration

- `FinancialSuite/appsettings.json`: `ConnectionStrings:DefaultConnection`, `AllowedHosts`, and the `Serilog` section (console + daily rolling file at `Logs/log-.txt`).
- `FinancialSuite/appsettings.Development.json`: default `Logging` levels.
- Configuration is read only in `AddInfrastructure(IConfiguration)` (connection string) and via `UseSerilog(... ReadFrom.Configuration ...)`. There is no options-pattern class. Do not change these files unless the task requires it.

### 9.2 Validation

- FluentValidation with `AddValidatorsFromAssembly(...).AddFluentValidationAutoValidation()` in `AddApplication()`. Validation runs automatically (SharpGrip auto-validation filter) on the bound command parameter before the controller action executes; handlers do not call validators.
- One `{Command}Validator : AbstractValidator<{Command}>` per body-bound command, in the same folder. ID-only commands (Delete) and queries have no validators.
- Rules in use: strings `.NotEmpty().MaximumLength(100)`; booleans `.NotNull()`. Each `RuleFor` is separated by a blank line with chained rules on indented lines:
  ```csharp
  // CreateDropDownCategoryCommandValidator.cs
  RuleFor(x => x.Value)
      .NotEmpty()
      .MaximumLength(100);
  ```
- Validators must be `public` and target their own command type.

### 9.3 Error handling

- Throw `NotFoundException(resourceType, resourceIdentifier)` (`FinancialSuite.Domain/Exceptions/NotFoundException.cs`) when an entity is missing. Two equivalent styles exist; match the file/feature you are in:
  - `?? throw new NotFoundException(nameof(DropDownCategory), id.ToString());` — `DropDownCategory` commands.
  - `if (data == null) throw new NotFoundException(nameof(DropDownCategory), id.ToString());` — `DropDownDetails` handlers.
- `FinancialSuite/Middlewares/ErrorHandlingMiddleware.cs` converts `NotFoundException` to **404 with the plain-text message** and any other exception to **500 "An internal error occurred."**, logging each. It is registered as scoped in `AddPresentation()` and added first in the pipeline in `Program.cs`.
- Handlers and controllers do not contain `try/catch`. Let exceptions propagate to the middleware. If a new error category is needed (e.g. 400/403), add a new exception type in `FinancialSuite.Domain/Exceptions/` and a matching `catch` in `ErrorHandlingMiddleware` rather than catching in handlers — and only when the task requires it.

### 9.4 Logging

- Serilog (`Serilog.AspNetCore`) hosts the logging pipeline; `app.UseSerilogRequestLogging()` logs requests. Code logs through `ILogger<T>`.
- Every handler injects `ILogger<TSelf>` but none currently writes a log. Keep injecting `ILogger<{ThisHandler}>` in new handlers for consistency (use the handler's own type). Logging is optional; when you log, use message templates with named placeholders as in `RequestTimeLoggingMiddleware.cs`:
  ```csharp
  logger.LogInformation("Request [{verb}] at {path} took longer than 4 seconds ({time}s).", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds/1000);
  ```
- Never use `Console.WriteLine` for diagnostics (see §13).

## 10. Rules for Modifying Existing Code

- Make the smallest change that satisfies the requirement; preserve existing behavior unless the task explicitly changes it.
- Follow the conventions of the file you are editing (namespace style, guard-clause style, `[FromRoute]` usage, handler visibility), even if another file does it differently.
- Reuse existing repositories, profiles, DTOs, `NotFoundException`, and `IUserContext` instead of creating parallel versions. Extend an existing `{Feature}Profile` rather than adding a second profile for the same feature.
- Do not change public API contracts (routes, verbs, status codes, request/response property names) unless required — including quirks such as `Update` returning `CreatedAtAction`.
- Do not modify `appsettings*.json`, `launchSettings.json`, `.csproj`/`.slnx`, migrations, the model snapshot, or seed data without a requirement.
- Do not reformat whole files, reorder usings, convert namespace styles, or strip template `using System.*` blocks in code you are not otherwise changing.
- Remove using directives that your own change made unnecessary.
- When you touch a file containing a defect from §13, leave it unless fixing it is in scope; mention it in your summary instead.

## 11. Rules for Adding New Functionality

1. **Locate similar functionality.** For CRUD on a top-level resource use `DropDownCategory`; for a child resource nested under a parent route use `DropDownDetails`; for current-user/Identity work use `Users`.
2. **Trace the full path**: controller action → command/query → validator → handler → repository interface → repository implementation → profile/DTO.
3. **Identify the pattern** for that slice (§5.1, §5.2, §6.1).
4. **Reuse** existing repositories, `NotFoundException`, `IUserContext`, and profile mapping rules.
5. **Place files** per §2.2 / §2.3, one type per file, namespace matching folder.
6. **Match conventions**: naming (§3.4), primary-constructor DI, `= string.Empty` initializers, FluentValidation rules, `IsActive` filtering in handlers, `DateTime.Now` audit dates in profiles, `"MM-dd-yyyy"` string dates in GetAll DTOs, `[Authorize]` on controllers.
7. **Register** only what is not auto-scanned: repositories/seeders in `AddInfrastructure`, explicit services in `AddApplication`, middlewares in `AddPresentation` + `Program.cs`.
8. **Do not add** new libraries, layers (services, unit of work, generic repository, result types), or a different validation/mapping/error approach.
9. **Verify** the solution builds (`dotnet build FinancialSuite.slnx`). There are no automated tests; if the task warrants it, add example calls to `FinancialSuite/FinancialSuite.API.http` matching its existing format.

## 12. Verified Examples from This Repository

| Convention | File | What it demonstrates |
|---|---|---|
| Controller template | `FinancialSuite/Controllers/DropDownCategoryController.cs` | `[ApiController]`/`[Authorize]`/`[Route]`, `IMediator` primary ctor, route id → command property, `Ok`/`CreatedAtAction`/`NoContent` |
| Nested-resource controller | `FinancialSuite/Controllers/DropDownDetailsController.cs` | Parent id in controller route (`api/dropdown/category/{id}/details`), child id in action route |
| Create command slice | `FinancialSuite.Application/DropDownCategory/Commands/Create/` | Command (`IRequest<Guid>`), handler (map → `CreateAsync`), validator |
| Update slice | `FinancialSuite.Application/DropDownCategory/Commands/Update/UpdateDropDownCategoryCommandHandler.cs` | `GetByIdAsync ?? throw NotFoundException`, `mapper.Map(request, data)`, `SaveChanges()` |
| ID-only command | `FinancialSuite.Application/DropDownCategory/Commands/Delete/DeleteDropDownCategoryCommand.cs` | Primary-constructor request with get-only property |
| Query + in-memory `IsActive` filter | `FinancialSuite.Application/DropDownCategory/Queries/GetAll/GetAllDropDownCategoryQueryHandler.cs` | Repository → `Where(x => x.IsActive)` → `mapper.Map<IEnumerable<Dto>>` |
| Child lookup through parent | `FinancialSuite.Application/DropDownDetails/Queries/GetById/GetByIdDropDownDetailsQueryHandler.cs` | Load category, find detail in `DropDownDetails`, throw `NotFoundException` for each missing level |
| AutoMapper profile | `FinancialSuite.Application/DropDownCategory/Dtos/DropDownCategoryProfile.cs` | Date formatting, `DateTime.Now` audit dates, `IsActive = true` on create |
| Repository | `FinancialSuite.Infrastructure/Repositories/DropDownCategoryRepository.cs` | Primary-ctor `dbContext`, `Include` on get-by-id, save inside Create/Delete, expression-bodied `SaveChanges` |
| Entity relationship config | `FinancialSuite.Infrastructure/Persistence/MyApplicationDbContext.cs` | `internal DbSet`, fluent `HasOne/WithMany/HasForeignKey/OnDelete(NoAction)` |
| DI registration per layer | `FinancialSuite.Application/Extensions/ServiceCollectionExtension.cs`, `FinancialSuite.Infrastructure/Extensions/ServiceCollectionExtensions.cs`, `FinancialSuite/Controllers/Extensions/WebApplicationBuilderExtensions.cs` | Assembly scanning vs explicit `AddScoped` |
| Global error handling | `FinancialSuite/Middlewares/ErrorHandlingMiddleware.cs` | `IMiddleware`, `NotFoundException` → 404, others → 500 |
| Current user | `FinancialSuite.Application/Users/UserContext.cs`, `FinancialSuite.Application/Users/Commands/UpdateUserDetailsCommandHandler.cs` | Claims → `CurrentUser`; Identity `IUserStore<User>` update |

## 13. Practices to Avoid

### 13.1 Existing defects — do not copy (fix only if the task asks)

| Location | Issue | In new code, instead |
|---|---|---|
| `FinancialSuite.Application/DropDownDetails/Commands/Toggle/ToggleDropDownDetailsCommandValidator.cs` | `internal` and validates `ToggleDropDownCategoryCommand` instead of `ToggleDropDownDetailsCommand`; FluentValidation's assembly scan skips internal types by default, so it is likely never registered | `public` validator targeting its own command |
| `FinancialSuite.Application/DropDownCategory/Commands/Update/UpdateDropDownCategoryCommandHandler.cs`, `FinancialSuite.Application/DropDownDetails/Commands/Toggle/ToggleDropDownDetailsCommandHandler.cs` | `ILogger<T>` typed with a different handler (copy-paste) | `ILogger<{ThisHandler}>` |
| `FinancialSuite.Application/DropDownDetails/Commands/Create/CreateDropDownDetailsCommandHandler.cs` | Returns `DropDownCategoryId` instead of the new detail id; `CreatedAtAction` in `DropDownDetailsController.Create` therefore gets the wrong `detailId` | Return the id produced by `CreateAsync` |
| `FinancialSuite.Application/DropDownCategory/Queries/GetById/GetByIdDropDownCategoryHandler.cs` | `data!` on a possibly-null result → `NullReferenceException` (500) instead of 404 | Check for null and throw `NotFoundException` |
| `FinancialSuite.Application/DropDownDetails/Commands/Toggle/ToggleDropDownDetailsCommandHandler.cs` | No not-found check on the detail before mapping | Throw `NotFoundException` when the child is missing |
| `FinancialSuite.Infrastructure/Repositories/DropDownDetailsRepository.cs` | `Console.WriteLine` debug output in `CreateAsync` | `ILogger<T>` or nothing |
| `FinancialSuite.Infrastructure/Seeders/DropDownDetailsSeeder/DropDownDetailsSeeder.cs` | `catch (Exception ex) { throw ex; }` resets the stack trace; unused `list` variable | Omit the try/catch (or `throw;`) |
| `FinancialSuite/Controllers/DropDownDetailsController.cs` | No `[Authorize]`, unlike `DropDownCategoryController`; unused `DropDownCategory.Commands.Toggle` using | Add `[Authorize]` on new controllers |
| `FinancialSuite.Infrastructure/Extensions/ServiceCollectionExtensions.cs` | `EnableSensitiveDataLogging()` is always on, not just in Development | Do not add more sensitive-data logging |
| `FinancialSuite/Middlewares/ErrorHandlingMiddleware.cs` | `logger.LogWarning(notFound.Message)` passes runtime text as the message template | Use a constant template with placeholders |
| `FinancialSuite.Application/DropDownDetails/Dtos/DropDownDetailsDto.cs` | Empty, unused class | Do not create placeholder DTOs |
| `FinancialSuite/FinancialSuite.API.http` | Contains a hard-coded bearer token | Never commit real tokens/credentials |

### 13.2 General

- Reimplementing what exists (a second not-found exception, a second current-user helper, manual mapping when a profile exists).
- Competing patterns: services alongside MediatR handlers, data annotations validation alongside FluentValidation, `try/catch` in handlers alongside the middleware, `DbContext` in handlers.
- Adding NuGet packages or new projects without a requirement.
- Unrelated refactors, renames, or whole-file reformatting.
- Editing configuration, migrations, snapshot, or seed data without a requirement.
- Changing existing routes, verbs, status codes, or DTO shapes unintentionally.

## 14. Checklist for Future Coding Tasks

**Before coding**
- [ ] Read this document, including §0, before starting the task.
- [ ] Confirmed the exact scope of the request; asked about any ambiguity that materially affects the implementation (§0.4).
- [ ] Found the closest existing slice (`DropDownCategory`, `DropDownDetails`, or `Users`) and read its controller, command/query, validator, handler, repository, and profile.
- [ ] Confirmed which entity/table/repository already exists (check `FinancialSuite.Domain/Entities`, `MyApplicationDbContext`, the model snapshot).
- [ ] Identified what can be reused and what genuinely needs to be added.
- [ ] Checked §13 so no known defect gets copied.

**While coding**
- [ ] Files placed per §2.2/§2.3; one type per file; namespace = folder path; block-scoped namespace.
- [ ] Names follow §3.4; entities referenced as `Domain.Entities.X` inside same-named feature namespaces.
- [ ] Primary-constructor DI; `ILogger<ThisType>`; `= string.Empty` on strings.
- [ ] Validation via a `public` FluentValidation validator; not-found via `NotFoundException`; no `try/catch` in handlers.
- [ ] Mapping in the feature profile; audit dates and `IsActive` set as in §6.3.
- [ ] New repositories/seeders registered in `AddInfrastructure`; new controllers have `[Authorize]`.
- [ ] No new packages, layers, or patterns; no unrelated edits; no template `using System.*` block in new files.

**After coding**
- [ ] `dotnet build FinancialSuite.slnx` succeeds with no new warnings introduced by your change.
- [ ] Reviewed the diff: only task-related files changed; config/migrations untouched unless required; generated migrations reviewed for unintended drops.
- [ ] Public API contracts unchanged unless the task required it.
- [ ] Reported any pre-existing defects you noticed but did not fix.
- [ ] Nothing was implemented beyond the requested scope without approval (§0.1–§0.4).
- [ ] Final report given as described in §0.7.
