namespace MusicApi.Features.Bands.GetBand;

public record Response(
    int Id, 
    string Name, 
    string Country, 
    int FoundationYear
);
