namespace MusicApi.Features.Bands.GetBand;

public record Request(
    [FromRoute] int Id,
    [FromServices] DataSource Data
);
