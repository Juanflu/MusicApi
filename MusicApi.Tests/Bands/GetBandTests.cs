namespace MusicApi.Tests.Bands;

using MusicApi.Features.Bands.GetBand;

public class GetBandTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetBand_ReturnsOkForExistingId()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/bands/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBand_ReturnsCorrectBand()
    {
        var client = factory.CreateClient();

        var band = await client.GetFromJsonAsync<Response>("/bands/2", JsonOptions);

        Assert.NotNull(band);
        Assert.Equal(2, band.Id);
        Assert.Equal("Iron Maiden", band.Name);
        Assert.Equal("UK", band.Country);
        Assert.Equal(1975, band.FoundationYear);
    }

    [Fact]
    public async Task GetBand_Returns404ForNonExistentId()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/bands/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
