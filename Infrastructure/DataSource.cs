namespace MusicApi.Infrastructure;

public class DataSource
{
    public List<BandData> Bands { get; }
    public List<AlbumData> Albums { get; }

    public DataSource()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data.json");
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var root = JsonSerializer.Deserialize<Root>(json, options)
            ?? throw new InvalidOperationException("Failed to load data.json");
        Bands = root.Bands;
        Albums = root.Albums;
    }

    private record Root(
        [property: JsonPropertyName("bands")]  List<BandData>  Bands,
        [property: JsonPropertyName("albums")] List<AlbumData> Albums
    );
}

public record BandData(int Id, string Name, string Country, int FoundationYear);
public record AlbumData(int Id, string Title, DateOnly ReleaseDate, string Genre, int BandId);
