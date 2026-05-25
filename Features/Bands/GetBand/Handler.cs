namespace MusicApi.Features.Bands.GetBand;

public record Handler() : GetHandlerAsync<Request>("/bands/{id}")
{
    protected override RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
        => builder
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetBand")
            .WithTags("Bands");

    protected override Task<IResult> HandleAsync(Request query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var band = query.Data.Bands.FirstOrDefault(b => b.Id == query.Id);
        
        return Task.FromResult(band is null
            ? Results.NotFound()
            : Results.Ok(new Response(band.Id, band.Name, band.Country, band.FoundationYear)));
    }
}
