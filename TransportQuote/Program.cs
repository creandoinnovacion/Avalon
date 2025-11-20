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
    var locations = locationService.GetLocations().ToDictionary(l => l.Id);
    if (!locations.TryGetValue(request.FromId, out var from) || !locations.TryGetValue(request.ToId, out var to))
    {
        return Results.BadRequest(new RouteResponseDto
        {
            Success = false,
            Message = "Ubicaciones inválidas para la ruta."
        });
    }

    var orderedPoints = new List<Location> { from };
    if (request.Stops is { Count: > 0 })
    {
        foreach (var stopId in request.Stops)
        {
            if (locations.TryGetValue(stopId, out var stop))
            {
                orderedPoints.Add(stop);
            }
        }
    }
    orderedPoints.Add(to);

    Console.WriteLine($"[Route] Origen: {from.Name} ({from.Latitude}, {from.Longitude})");
    Console.WriteLine($"[Route] Destino: {to.Name} ({to.Latitude}, {to.Longitude})");
    if (request.Stops is { Count: > 0 })
    {
        Console.WriteLine("[Route] Paradas:");
        foreach (var stop in orderedPoints.Skip(1).Take(orderedPoints.Count - 2))
        {
            Console.WriteLine($" - {stop.Name} ({stop.Latitude}, {stop.Longitude})");
        }
    }

    var aggregatedSegments = new List<RouteSegmentDto>();
    double totalDistance = 0;
    double totalDuration = 0;

    for (int i = 0; i < orderedPoints.Count - 1; i++)
    {
        var segmentResult = await routePlanner.BuildRouteAsync(orderedPoints[i], orderedPoints[i + 1], cancellationToken);
        if (!segmentResult.Success)
        {
            return Results.Ok(new RouteResponseDto
            {
                Success = false,
                Message = segmentResult.Message
            });
        }

        aggregatedSegments.AddRange(segmentResult.Segments);
        totalDistance += segmentResult.DistanceKm;
        totalDuration += segmentResult.DurationMinutes;
    }

    return Results.Ok(new RouteResponseDto
    {
        Success = true,
        DistanceKm = totalDistance,
        DurationMinutes = totalDuration,
        Segments = aggregatedSegments
    });
});

app.Run();
