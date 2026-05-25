namespace MusicApi.Features.Albums.GetAlbum;

public record Request(int Id, [FromServices] DataSource Data);
