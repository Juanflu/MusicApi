namespace MusicApi.Features.Bands.GetBand;

public record Request(
    int Id, 
    [FromServices] DataSource Data
);
