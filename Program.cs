var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DataSource>();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapEndpoints();
app.Run();

public partial class Program { }
