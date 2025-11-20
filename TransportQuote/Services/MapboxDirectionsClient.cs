using System.Globalization;
using System.Linq;
using System.Text.Json;
using TransportQuote.Models;

namespace TransportQuote.Services;

public class MapboxDirectionsClient
{
    private readonly HttpClient _httpClient;
    private readonly MapboxOptions _options;

    public MapboxDirectionsClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _options = configuration.GetSection("Mapbox").Get<MapboxOptions>() ?? new MapboxOptions();
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.AccessToken);

    public async Task<RouteSegmentData?> GetRouteAsync(GeoCoordinate start, GeoCoordinate end, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return null;
        }

        var preferredProfile = string.IsNullOrWhiteSpace(_options.Profile) ? "driving-traffic" : _options.Profile;
        var profiles = new[]
        {
            preferredProfile,
            "driving-traffic",
            "driving"
        }.Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var profile in profiles)
        {
            var segment = await QueryRouteAsync(profile, start, end, cancellationToken);
            if (segment != null)
            {
                return segment;
            }
        }

        return null;
    }

    private async Task<RouteSegmentData?> QueryRouteAsync(string profile, GeoCoordinate start, GeoCoordinate end, CancellationToken cancellationToken)
    {
        var supportsTraffic = profile.Contains("traffic", StringComparison.OrdinalIgnoreCase);
        var annotations = supportsTraffic ? "duration,duration_typical" : "duration";
        var url =
            $"https://api.mapbox.com/directions/v5/mapbox/{profile}/{start.Longitude.ToString(CultureInfo.InvariantCulture)},{start.Latitude.ToString(CultureInfo.InvariantCulture)};{end.Longitude.ToString(CultureInfo.InvariantCulture)},{end.Latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"?alternatives=false&geometries=geojson&annotations={annotations}&overview=full&access_token={_options.AccessToken}";

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
        if (!route.TryGetProperty("geometry", out var geometry) ||
            !geometry.TryGetProperty("coordinates", out var coordinatesElement))
        {
            return null;
        }

        var segment = new RouteSegmentData { Mode = "land" };
        foreach (var coordinate in coordinatesElement.EnumerateArray())
        {
            if (coordinate.GetArrayLength() >= 2)
            {
                var lon = coordinate[0].GetDouble();
                var lat = coordinate[1].GetDouble();
                segment.Coordinates.Add(new GeoCoordinate(lat, lon));
            }
        }

        if (segment.Coordinates.Count == 0)
        {
            return null;
        }

        segment.DistanceKm = route.TryGetProperty("distance", out var distance) ? distance.GetDouble() / 1000d : 0;
        segment.DurationMinutes = route.TryGetProperty("duration", out var duration) ? duration.GetDouble() / 60d : 0;

        if (supportsTraffic && route.TryGetProperty("duration_typical", out var durationTypical))
        {
            var typicalMinutes = durationTypical.GetDouble() / 60d;
            if (typicalMinutes > 0)
            {
                segment.TrafficDelayMinutes = Math.Max(segment.DurationMinutes - typicalMinutes, 0);
            }
        }
        return segment;
    }
}

public class MapboxOptions
{
    public string AccessToken { get; set; } = string.Empty;
    public string Profile { get; set; } = "driving-traffic";
}
