using TransportQuote.Models;
using TransportQuote.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<TransportQuote.Services.LocationService>();
builder.Services.AddHttpClient<OpenRouteServiceClient>();
builder.Services.AddScoped<RoutePlanner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapPost("/api/routes", async (RouteRequest request, LocationService locationService, RoutePlanner routePlanner, CancellationToken cancellationToken) =>
{
    var locations = locationService.GetLocations().ToList();
    var from = locations.FirstOrDefault(l => l.Id == request.FromId);
    var to = locations.FirstOrDefault(l => l.Id == request.ToId);

    if (from == null || to == null)
    {
        return Results.BadRequest(new RouteResponseDto
        {
            Success = false,
            Message = "Ubicaciones inválidas para la ruta."
        });
    }

    var result = await routePlanner.BuildRouteAsync(from, to, cancellationToken);
    if (!result.Success)
    {
        return Results.Ok(new RouteResponseDto
        {
            Success = false,
            Message = result.Message
        });
    }

    return Results.Ok(new RouteResponseDto
    {
        Success = true,
        DistanceKm = result.DistanceKm,
        DurationMinutes = result.DurationMinutes,
        Segments = result.Segments
    });
});

app.Run();
