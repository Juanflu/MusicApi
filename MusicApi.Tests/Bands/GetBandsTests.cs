namespace MusicApi.Tests.Bands;

using MusicApi.Features.Bands.GetBands;

public class GetBandsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetBands_ReturnsOk()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/bands");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBands_ReturnsAllBands()
    {
        var client = factory.CreateClient();

        var bands = await client.GetFromJsonAsync<List<Response>>("/bands", JsonOptions);

        Assert.NotNull(bands);
        Assert.Equal(5, bands.Count);
    }

    [Fact]
    public async Task GetBands_ReturnsCorrectBandData()
    {
        var client = factory.CreateClient();

        var bands = await client.GetFromJsonAsync<List<Response>>("/bands", JsonOptions);

        Assert.NotNull(bands);
        var metallica = bands.Single(b => b.Id == 1);
        Assert.Equal("Metallica", metallica.Name);
        Assert.Equal("USA", metallica.Country);
        Assert.Equal(1981, metallica.FoundationYear);
    }
}
