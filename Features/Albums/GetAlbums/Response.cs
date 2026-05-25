namespace MusicApi.Features.Albums.GetAlbums;

public record Response(int Id, string Title, DateOnly ReleaseDate, string Genre, int BandId);
