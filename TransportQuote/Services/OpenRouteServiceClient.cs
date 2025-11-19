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

        var startParam = $"{start.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{start.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        var endParam = $"{end.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{end.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v2/directions/{profile}?api_key={_options.ApiKey}&start={startParam}&end={endParam}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
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
}

public class RouteSegmentData
{
    public string Mode { get; set; } = "land";
    public List<GeoCoordinate> Coordinates { get; set; } = new();
    public double DistanceKm { get; set; }
    public double DurationMinutes { get; set; }
}
