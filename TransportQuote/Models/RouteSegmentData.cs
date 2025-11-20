namespace TransportQuote.Models;

public class RouteSegmentData
{
    public string Mode { get; set; } = "land";
    public List<GeoCoordinate> Coordinates { get; set; } = new();
    public double DistanceKm { get; set; }
    public double DurationMinutes { get; set; }
    public double TrafficDelayMinutes { get; set; }
}
