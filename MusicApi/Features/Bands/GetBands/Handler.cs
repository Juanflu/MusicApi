namespace MusicApi.Features.Bands.GetBands;

public record Handler() : GetHandlerAsync<Request>("/bands")
{
    protected override RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
        => builder
            .Produces<IEnumerable<Response>>(StatusCodes.Status200OK)
            .WithName("GetBands")
            .WithTags("Bands");

    protected override Task<IResult> HandleAsync(Request query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bands = query.Data.Bands.Select(b => new Response(b.Id, b.Name, b.Country, b.FoundationYear));
        return Task.FromResult<IResult>(Results.Ok(bands));
    }
}
