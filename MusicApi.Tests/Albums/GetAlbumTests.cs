namespace MusicApi.Tests.Albums;

using MusicApi.Features.Albums.GetAlbum;

public class GetAlbumTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetAlbum_ReturnsOkForExistingId()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/albums/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAlbum_ReturnsCorrectAlbum()
    {
        var client = factory.CreateClient();

        var album = await client.GetFromJsonAsync<Response>("/albums/5", JsonOptions);

        Assert.NotNull(album);
        Assert.Equal(5, album.Id);
        Assert.Equal("OK Computer", album.Title);
        Assert.Equal(new DateOnly(1997, 5, 21), album.ReleaseDate);
        Assert.Equal("Alternative Rock", album.Genre);
        Assert.Equal(3, album.BandId);
    }

    [Fact]
    public async Task GetAlbum_Returns404ForNonExistentId()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/albums/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
