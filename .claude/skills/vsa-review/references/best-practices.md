# VSA Best Practices — MinApiLib.Endpoints

Reference guide for Vertical Slice Architecture with ASP.NET Core Minimal APIs and MinApiLib.Endpoints.

---

## 1. Core Principles

### 1.1 A vertical slice is a feature

A **vertical slice** represents a single business capability (use case), for example:

- `CreateBand`
- `GetBand`
- `UpdateBand`
- `DeleteBand`

Each slice owns: the HTTP route and verb, OpenAPI metadata, request binding, validation, business orchestration, persistence interaction, response mapping, and tests.

> There are no horizontal layers such as `Controllers`, `Services`, or `Repositories`.

### 1.2 One slice = One endpoint record

When using `MinApiLib.Endpoints`, a slice is implemented as a single `record` type that:

1. Inherits from a verb-specific base type (`GetHandlerAsync<TRequest>`, `PostHandlerAsync<TRequest>`, etc.).
2. Configures routes and metadata in `Configure(...)`.
3. Implements the handler logic in `HandleAsync(...)`.

---

## 2. Folder and Namespace Structure

```text
MusicApi/
└── Features/
    └── Bands/
        ├── CreateBand/
        │   ├── Handler.cs
        │   ├── Request.cs          // Optional
        │   ├── Response.cs         // Optional
        │   ├── Validator.cs        // Optional
        │   ├── Mapping.cs          // Optional
        │   └── DatabaseAccess.cs   // Optional
        └── GetBand/
            └── Handler.cs
```

**Rules:**

- One folder per feature, named after the operation (verb + noun).
- Feature folders must be independent — no cross-imports between slices.
- `Extensions/` is allowed only for true cross-cutting concerns (auth, telemetry).
- Avoid generic folders: `Endpoints/`, `Controllers/`, `Services/`, `Repositories/`.

---

## 3. Choosing the MinApiLib Base Type

| HTTP verb | Base type |
|-----------|-----------|
| GET | `GetHandlerAsync<Request>("/bands/{id}")` |
| POST | `PostHandlerAsync<Request>("/bands")` |
| PUT | `PutHandlerAsync<Request>("/bands/{id}")` |
| DELETE | `DeleteHandlerAsync<Request>("/bands/{id}")` |

---

## 4. Metadata and Routing (Configure)

`Configure(RouteHandlerBuilder builder)` is responsible for all endpoint metadata:

```csharp
protected override RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
    => builder
        .Produces<BandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetBand")
        .WithTags("Bands");
```

For endpoints with validation:

```csharp
protected override RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
    => builder
        .Produces<BandResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("CreateBand")
        .WithTags("Bands")
        .WithValidation();
```

**Required in every Configure:**

- `.WithName(...)` — must be unique; used by `CreatedAtRoute` for POST.
- `.WithTags(...)` — organizes the OpenAPI spec.
- `.Produces(...)` for every status code the handler can return.
- `.WithValidation()` — add when a `Validator.cs` exists in the same slice.

---

## 5. Handler Logic (HandleAsync)

### 5.1 Responsibilities

`HandleAsync` implements the use case:
1. Validate cross-entity constraints not covered by the validator.
2. Execute the business action.
3. Persist changes (if any).
4. Map to a `Response` type.
5. Return the correct HTTP result.

### 5.2 CancellationToken Rules

- Always accept `CancellationToken` as the last parameter.
- First statement must be `cancellationToken.ThrowIfCancellationRequested()`.
- Pass the token to **every** asynchronous I/O call.

```csharp
public override async Task<IResult> HandleAsync(Request request, CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    var band = await request.Data.Bands
        .FindAsync(request.Id, cancellationToken);

    if (band is null)
        return Results.NotFound();

    return Results.Ok(new Response(band.Name, band.Genre));
}
```

---

## 6. Request Binding

### 6.1 Binding Options

Follows Minimal API rules: route parameters, query string, headers, body, and services. Group them into a `Request` record:

```csharp
public readonly record struct Request(
    [FromRoute] Guid Id,
    [FromServices] DataSource Data);
```

### 6.2 Rules

- Request models are **slice-local** — never define a shared request type used by multiple operations.
- Do not reuse request models across different features, even if the shapes look identical.
- Use `[FromServices]` for injected dependencies (`DataSource`, validators, etc.).

---

## 7. Response Contracts and Mapping

**Always define a `Response` type**, even when its shape matches the internal model today. This prevents domain entity leakage and allows the API to evolve independently.

> **Golden Rule:** Domain or persistence entities (`BandData`, `AlbumData`, EF entities) must never be returned directly from a handler — always map to a dedicated `Response` record.

```csharp
// Wrong — leaks internal type
return Results.Ok(band); // band is BandData

// Correct — maps to Response
return Results.Ok(new Response(band.Id, band.Name, band.Genre));
```

---

## 8. HTTP Semantics and Results

| Operation | Status Code | Implementation |
|-----------|-------------|----------------|
| **Read (single)** | 200 OK / 404 | `Results.Ok(response)` / `Results.NotFound()` |
| **Read (collection)** | 200 OK | `Results.Ok(list)` — empty list is 200, not 404 |
| **Create** | 201 Created | `Results.CreatedAtRoute("RouteName", new { id }, response)` |
| **Update** | 200 OK or 204 | `Results.Ok(response)` or `Results.NoContent()` — pick one |
| **Delete** | 204 No Content | `Results.NoContent()` — no body |
| **Validation failure** | 400 Bad Request | `Results.BadRequest(errors)` — standardize the error shape |
| **Conflict / duplicate** | 409 Conflict | `Results.Conflict(...)` — for duplicates or state mismatches |

**Notes:**
- For POST, `Results.CreatedAtRoute` requires the route name to match `WithName(...)` in `Configure`.
- Choose either 200 or 204 for PUT and apply that convention consistently across all slices.

---

## 9. Testing Strategy

### 9.1 Integration Tests (Best Practice)

Default to integration tests that cover the whole slice (validation, data access, mapping). Use `WebApplicationFactory<Program>` for an in-memory HTTP server:

```csharp
public class GetBandTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetBandTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Returns_200_when_band_exists()
    {
        var response = await _client.GetAsync("/bands/1");
        response.EnsureSuccessStatusCode();
    }
}
```

Alternatively, instantiate the handler directly and call `DelegateAsync`:

```csharp
[Fact]
public async Task Test_Demo()
{
    var target = new Handler();
    var request = new Request(/* ... */);
    var result = await target.DelegateAsync(request, CancellationToken.None);
    Assert.IsType<Ok<Response>>(result);
}
```

### 9.2 Unit Tests

Reach for unit tests only for truly isolated logic (complex calculations, pure mappers). They are not a substitute for slice-level verification.

**Do not mock `DataSource` or repository interfaces** — mocked tests can pass while real persistence breaks. Use the real data fixture.

---

## 10. What to Avoid

| Anti-pattern | Why it violates VSA |
|--------------|---------------------|
| Giant `Program.cs` with endpoint logic | Logic belongs in the slice Handler |
| Splitting a slice across multiple files without a clear reason | Reduces cohesion |
| Reusing DTOs across unrelated features | Couples independent slices |
| Returning domain entities (`BandData`, etc.) directly | Leaks internal model into the API contract |
| `Controllers/`, `Services/`, `Repositories/` folders | Horizontal layers break the vertical slice model |
| Mocking the data layer in integration tests | Hides real persistence bugs |

---

## 11. Tips & Tricks

### Dependency Injection

Use `[RegisterInIServiceCollection]` to wire services automatically instead of registering them manually in `Program.cs`:

```csharp
[RegisterInIServiceCollection(ServiceLifetime.Singleton)]
public class DataSource { ... }
```

### CancellationToken — 499 Client Closed Request

Use `app.CatchOperationCanceled()` in `Program.cs` to translate `OperationCanceledException` into **499 Client Closed Request** instead of a 500 error:

```csharp
app.CatchOperationCanceled();
```

### FluentValidation

Integrate fluid validations using `.WithValidation()` in `Configure` and register all validators at startup:

```csharp
services.AddValidation(); // auto-discovers all AbstractValidator<T> in the assembly
```

```csharp
// CreateBand/Validator.cs
public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(200);
    }
}
```
