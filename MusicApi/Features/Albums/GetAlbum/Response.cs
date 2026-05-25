namespace MusicApi.Features.Albums.GetAlbum;

public record Response(int Id, string Title, DateOnly ReleaseDate, string Genre, int BandId);
