namespace MusicApi.Features.Albums.GetAlbum;

public record Request(
    [FromRoute] int Id, 
    [FromServices] DataSource Data
);
