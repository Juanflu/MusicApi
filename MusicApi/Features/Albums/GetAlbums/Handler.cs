namespace MusicApi.Features.Albums.GetAlbums;

public record Handler() : GetHandlerAsync<Request>("/albums")
{
    protected override RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
        => builder
            .Produces<IEnumerable<Response>>(StatusCodes.Status200OK)
            .WithName("GetAlbums")
            .WithTags("Albums");

    protected override Task<IResult> HandleAsync(Request query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var albums = query.Data.Albums.Select(a => new Response(a.Id, a.Title, a.ReleaseDate, a.Genre, a.BandId));
        return Task.FromResult<IResult>(Results.Ok(albums));
    }
}
