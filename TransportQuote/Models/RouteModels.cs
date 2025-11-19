using System.Text.Json.Serialization;

namespace TransportQuote.Models;

public class RouteRequest
{
    public string FromId { get; set; } = string.Empty;
    public string ToId { get; set; } = string.Empty;
    public List<string> Stops { get; set; } = new();
}

public class RouteSegmentDto
{
    public string Mode { get; set; } = "land";

    [JsonPropertyName("coordinates")]
    public List<double[]> Coordinates { get; set; } = new();
}

public class RouteResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public double DistanceKm { get; set; }
    public double DurationMinutes { get; set; }
    public List<RouteSegmentDto> Segments { get; set; } = new();
}
