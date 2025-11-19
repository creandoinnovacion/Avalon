using System.Globalization;
using System.Text.Json;
using TransportQuote.Models;

namespace TransportQuote.Services;

public class OpenRouteServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouteServiceOptions _options;

    public OpenRouteServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _options = configuration.GetSection("OpenRouteService").Get<OpenRouteServiceOptions>() ?? new OpenRouteServiceOptions();
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<RouteSegmentData?> GetRouteAsync(GeoCoordinate start, GeoCoordinate end, string profile = "driving-car", CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return null;
        }

        try
        {
            var orsSegment = await TryOpenRouteServiceAsync(start, end, profile, cancellationToken);
            if (orsSegment != null)
            {
                return orsSegment;
            }
        }
        catch (HttpRequestException)
        {
            // Ignored so we can fallback to OSRM
        }

        try
        {
            return await TryOsrmFallbackAsync(start, end, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<RouteSegmentData?> TryOpenRouteServiceAsync(GeoCoordinate start, GeoCoordinate end, string profile, CancellationToken cancellationToken)
    {
        var startParam = $"{start.Longitude.ToString(CultureInfo.InvariantCulture)},{start.Latitude.ToString(CultureInfo.InvariantCulture)}";
        var endParam = $"{end.Longitude.ToString(CultureInfo.InvariantCulture)},{end.Latitude.ToString(CultureInfo.InvariantCulture)}";
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v2/directions/{profile}?api_key={_options.ApiKey}&start={startParam}&end={endParam}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
        {
            return null;
        }

        var feature = features[0];
        if (!feature.TryGetProperty("geometry", out var geometryElement) ||
            !geometryElement.TryGetProperty("coordinates", out var geometry))
        {
            return null;
        }

        var segment = new RouteSegmentData { Mode = "land" };
        foreach (var coordinate in geometry.EnumerateArray())
        {
            if (coordinate.GetArrayLength() >= 2)
            {
                var lon = coordinate[0].GetDouble();
                var lat = coordinate[1].GetDouble();
                segment.Coordinates.Add(new GeoCoordinate(lat, lon));
            }
        }

        if (!feature.TryGetProperty("properties", out var properties) ||
            !properties.TryGetProperty("summary", out var summary))
        {
            return null;
        }

        segment.DistanceKm = summary.GetProperty("distance").GetDouble() / 1000d;
        segment.DurationMinutes = summary.GetProperty("duration").GetDouble() / 60d;
        return segment;
    }

    private async Task<RouteSegmentData?> TryOsrmFallbackAsync(GeoCoordinate start, GeoCoordinate end, CancellationToken cancellationToken)
    {
        var lonLatFormat = $"{start.Longitude.ToString(CultureInfo.InvariantCulture)},{start.Latitude.ToString(CultureInfo.InvariantCulture)};" +
                           $"{end.Longitude.ToString(CultureInfo.InvariantCulture)},{end.Latitude.ToString(CultureInfo.InvariantCulture)}";
        var url = $"https://router.project-osrm.org/route/v1/driving/{lonLatFormat}?overview=full&geometries=geojson";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
        {
            return null;
        }

        var route = routes[0];
        if (!route.TryGetProperty("geometry", out var geometryElement) ||
            !geometryElement.TryGetProperty("coordinates", out var coordinates))
        {
            return null;
        }

        var segment = new RouteSegmentData { Mode = "land" };
        foreach (var coordinate in coordinates.EnumerateArray())
        {
            if (coordinate.GetArrayLength() >= 2)
            {
                var lon = coordinate[0].GetDouble();
                var lat = coordinate[1].GetDouble();
                segment.Coordinates.Add(new GeoCoordinate(lat, lon));
            }
        }

        segment.DistanceKm = route.TryGetProperty("distance", out var distance) ? distance.GetDouble() / 1000d : 0;
        segment.DurationMinutes = route.TryGetProperty("duration", out var duration) ? duration.GetDouble() / 60d : 0;
        return segment;
    }
}

public class RouteSegmentData
{
    public string Mode { get; set; } = "land";
    public List<GeoCoordinate> Coordinates { get; set; } = new();
    public double DistanceKm { get; set; }
    public double DurationMinutes { get; set; }
}
