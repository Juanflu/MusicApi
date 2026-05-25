namespace MusicApi.Tests.Albums;

using MusicApi.Features.Albums.GetAlbums;

public class GetAlbumsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetAlbums_ReturnsOk()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/albums");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAlbums_ReturnsAllAlbums()
    {
        var client = factory.CreateClient();

        var albums = await client.GetFromJsonAsync<List<Response>>("/albums", JsonOptions);

        Assert.NotNull(albums);
        Assert.Equal(10, albums.Count);
    }

    [Fact]
    public async Task GetAlbums_ReturnsCorrectAlbumData()
    {
        var client = factory.CreateClient();

        var albums = await client.GetFromJsonAsync<List<Response>>("/albums", JsonOptions);

        Assert.NotNull(albums);
        var masterOfPuppets = albums.Single(a => a.Id == 1);
        Assert.Equal("Master of Puppets", masterOfPuppets.Title);
        Assert.Equal(new DateOnly(1986, 3, 3), masterOfPuppets.ReleaseDate);
        Assert.Equal("Heavy Metal", masterOfPuppets.Genre);
        Assert.Equal(1, masterOfPuppets.BandId);
    }
}
