namespace MusicApi.Features.Albums.GetAlbum;

public record Handler() : GetHandlerAsync<Request>("/albums/{id}")
{
    protected override RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
        => builder
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetAlbum")
            .WithTags("Albums");

    protected override Task<IResult> HandleAsync(Request query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var album = query.Data.Albums.FirstOrDefault(a => a.Id == query.Id);
        return Task.FromResult(album is null
            ? Results.NotFound()
            : Results.Ok(new Response(album.Id, album.Title, album.ReleaseDate, album.Genre, album.BandId)));
    }
}
