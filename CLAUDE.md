# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the API
dotnet run --project MusicApi/MusicApi

# Run all tests
dotnet test MusicApi.Tests/MusicApi.Tests.csproj

# Run a specific test class
dotnet test MusicApi.Tests/MusicApi.Tests.csproj --filter "FullyQualifiedName~GetBandTests"

# Build the solution
dotnet build MusicApi.sln
```

## Architecture

### Solution structure
```
MusicApi.sln
MusicApi/          ← main project (ASP.NET Core minimal API, net9.0)
MusicApi.Tests/    ← xUnit test project
```

### Endpoint pattern (MinApiLib.Endpoints)

Every endpoint lives in `MusicApi/Features/{Domain}/{Operation}/` and consists of three files:

- **Handler.cs** — inherits `GetHandlerAsync<Request>`, declares the route in the constructor, overrides `Configure` (OpenAPI metadata) and `HandleAsync` (business logic).
- **Request.cs** — a record whose parameters are either route/query values or `[FromServices]` injected services.
- **Response.cs** — a record representing the JSON response shape.

`app.MapEndpoints()` in `Program.cs` auto-discovers all handlers via reflection — no manual route registration needed. Adding a new endpoint means adding a new feature folder with these three files.

### Data layer

`DataSource` (singleton) reads `data.json` from `AppContext.BaseDirectory` at startup. It exposes `List<BandData> Bands` and `List<AlbumData> Albums`. All handlers receive it via `[FromServices] DataSource Data` in their `Request` record.

`data.json` must be present in the build output — both `.csproj` files are configured with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`.

### Testing

Tests use `WebApplicationFactory<Program>` (in-memory HTTP server, no mocks). `public partial class Program {}` at the bottom of `Program.cs` is required to make `Program` accessible to the test project.

Response deserialization uses `new JsonSerializerOptions { PropertyNameCaseInsensitive = true }` because ASP.NET Core serializes to camelCase but the C# records use PascalCase properties.
