---
name: vsa-review
description: This skill should be used when the user asks to "review the API", "check VSA compliance", "audit the architecture", "run a code review", "check best practices", or wants to verify that MusicApi feature slices follow Vertical Slice Architecture best practices with MinApiLib.Endpoints.
version: 1.0.0
---

# VSA Code Review — MusicApi

Perform a full code review of the MusicApi solution. Execute the three checks below in order and produce a structured report at the end.

For the authoritative VSA rules, consult `references/best-practices.md`.

---

## Check 1 — Build

Run:
```
dotnet build MusicApi.sln
```
Record whether it succeeded. List any errors or warnings verbatim.

---

## Check 2 — Tests

Run:
```
dotnet test MusicApi.Tests/MusicApi.Tests.csproj --logger "console;verbosity=normal"
```
Record total passed/failed. For any failing test include its name and the failure message.

Also inspect the test files themselves and flag:
- Tests that mock the data layer instead of using `WebApplicationFactory` (integration-first rule).
- Tests that do not call `DelegateAsync` or go through the HTTP stack — either pattern is acceptable, but mocking `DataSource` or service interfaces is a violation.

---

## Check 3 — VSA Architecture & Best Practices

Read every `.cs` file under `MusicApi/Features/` and verify each feature slice against the rules below.

### 3.1 Folder structure

- Layout must be `Features/{Domain}/{Operation}/` (e.g. `Features/Bands/GetBand/`).
- Allowed files per slice: `Handler.cs`, `Request.cs`, `Response.cs`, `Validator.cs`, `Mapping.cs`, `DatabaseAccess.cs`.
- Flag any horizontal-layer folders: `Controllers/`, `Services/`, `Repositories/`, `Endpoints/`.
- `Extensions/` is allowed only for true cross-cutting concerns (auth, telemetry); flag anything that belongs in a slice.

### 3.2 Handler.cs

- Must be a `record` inheriting from a MinApiLib verb base: `GetHandlerAsync<TRequest>`, `PostHandlerAsync<TRequest>`, `PutHandlerAsync<TRequest>`, or `DeleteHandlerAsync<TRequest>`.
- The route URL must be declared in the record constructor argument, not hardcoded elsewhere.
- Must override `Configure(RouteHandlerBuilder builder)` and include at minimum:
  - `.Produces(...)` for every expected status code.
  - `.WithName(...)` — used by `CreatedAtRoute` for POST endpoints.
  - `.WithTags(...)`.
  - `.WithValidation()` if a `Validator.cs` exists in the same slice.
- Must override `HandleAsync(TRequest request, CancellationToken cancellationToken)` returning `Task<IResult>`.
- First statement of `HandleAsync` must be `cancellationToken.ThrowIfCancellationRequested()`.
- The `CancellationToken` must be forwarded to **every** async I/O call inside `HandleAsync`.

### 3.3 Request.cs

- Must be a `record` or `readonly record struct`.
- Namespace must be `MusicApi.Features.{Domain}.{Operation}` — never shared across different operations.
- Binding attributes must be used where non-obvious: `[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromServices]`.
- Flag any `Request` type that is reused across more than one feature slice.

### 3.4 Response.cs

- Must exist for any endpoint that returns a body.
- Domain/persistence types (`BandData`, `AlbumData`, or any internal entity) must **never** be returned directly from a handler — always map to a dedicated `Response` type.
- A `Response` type must be defined even when its shape matches the internal model today (prevents future leakage).

### 3.5 HTTP semantics

| Verb | Expected result | Notes |
|------|----------------|-------|
| GET single resource | `Results.Ok(response)` on success | `Results.NotFound()` when resource is missing |
| GET collection | `Results.Ok(collection)` | Empty list is OK, never 404 |
| POST | `Results.CreatedAtRoute(routeName, ...)` | Route name must match `WithName` in Configure |
| PUT | `Results.Ok(...)` or `Results.NoContent()` | Pick one convention and stick to it |
| DELETE | `Results.NoContent()` | No body on success |
| Validation failure | `Results.BadRequest(...)` | Standardize the error shape |
| Conflict / duplicate | `Results.Conflict(...)` | 409 for state mismatches |

### 3.6 Namespace coherence

`Handler`, `Request`, and `Response` in the same slice must share the namespace `MusicApi.Features.{Domain}.{Operation}`.

### 3.7 What to avoid (flag as violations)

- Splitting a slice's logic across files that belong in other layers.
- Any use of `Program.cs` for endpoint logic beyond `app.MapEndpoints()`.
- Returning raw exceptions or stack traces in responses.
- DI services registered manually in `Program.cs` that have `[RegisterInIServiceCollection]` on them — they should auto-register.

---

## Report format

Output exactly this structure — use ✅ for pass and ❌ for each violation:

```
## Build
✅ Compiled successfully
(or list errors)

## Tests
✅ N passed, 0 failed
(or list failing tests with messages)
(or flag test violations: mocking data layer, missing integration coverage)

## Architecture

### MusicApi/Features/Bands/GetBands
✅ Handler — OK
✅ Request — OK
✅ Response — OK

### MusicApi/Features/Bands/GetBand
✅ Handler — OK
...

(repeat for every slice found)

### Summary
Total violations: N
(list each violation as: ❌ File path — Rule broken: explanation)
```

If there are no violations, end with: **All checks passed.**

---

## Additional Resources

- **`references/best-practices.md`** — Full VSA with MinApiLib.Endpoints guidelines (authoritative reference for edge cases and detailed rules).
