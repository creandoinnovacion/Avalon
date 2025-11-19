using TransportQuote.Models;

namespace TransportQuote.Services;

public class RoutePlanner
{
    private readonly OpenRouteServiceClient _openRouteServiceClient;

    private static readonly FerryConnection IslaMujeresConnection = new(
        Landmass.IslaMujeres,
        new GeoCoordinate(21.2324, -86.7340), // Isla Mujeres ferry
        new GeoCoordinate(21.1704, -86.8126)  // Puerto Juárez
    );

    private static readonly FerryConnection CozumelConnection = new(
        Landmass.Cozumel,
        new GeoCoordinate(20.5110, -86.9480), // San Miguel de Cozumel ferry
        new GeoCoordinate(20.6274, -87.0739)  // Playa del Carmen ferry
    );

    private static readonly FerryConnection ContoyConnection = new(
        Landmass.IslaContoy,
        new GeoCoordinate(21.5021, -86.7989),
        new GeoCoordinate(21.2469, -86.8102) // Costa Mujeres marina
    );

    private static readonly IReadOnlyDictionary<Landmass, FerryConnection> FerryConnections = new Dictionary<Landmass, FerryConnection>
    {
        [Landmass.IslaMujeres] = IslaMujeresConnection,
        [Landmass.Cozumel] = CozumelConnection,
        [Landmass.IslaContoy] = ContoyConnection
    };

    public RoutePlanner(OpenRouteServiceClient openRouteServiceClient)
    {
        _openRouteServiceClient = openRouteServiceClient;
    }

    public async Task<RouteComputationResult> BuildRouteAsync(Location from, Location to, CancellationToken cancellationToken)
    {
        var fromMass = DetermineLandmass(from);
        var toMass = DetermineLandmass(to);
        var plan = BuildPlan(fromMass, toMass, new GeoCoordinate(from.Latitude, from.Longitude), new GeoCoordinate(to.Latitude, to.Longitude));

        if (!plan.Any())
        {
            return RouteComputationResult.Failure("No se pudo construir la ruta.");
        }

        var segments = new List<RouteSegmentDto>();
        double totalDistance = 0;
        double totalDuration = 0;

        foreach (var step in plan)
        {
            if (step.Type == RouteStepType.Land)
            {
                var routeSegment = await _openRouteServiceClient.GetRouteAsync(step.Start, step.End, "driving-car", cancellationToken);
                if (routeSegment == null)
                {
                    return RouteComputationResult.Failure("No se pudo obtener una ruta detallada desde OpenRouteService. Verifica la API key.");
                }

                var coordinates = routeSegment.Coordinates;
                if (coordinates.Count == 0 || coordinates[^1] != step.End)
                {
                    coordinates.Add(step.End);
                }

                segments.Add(new RouteSegmentDto
                {
                    Mode = "land",
                    Coordinates = ConvertCoordinates(coordinates),
                    DistanceKm = routeSegment.DistanceKm
                });
                totalDistance += routeSegment.DistanceKm;
                totalDuration += routeSegment.DurationMinutes;
            }
            else if (step.Type == RouteStepType.Sea)
            {
                var seaDistance = GeoDistance(step.Start, step.End);
                segments.Add(new RouteSegmentDto
                {
                    Mode = "sea",
                    Coordinates = ConvertCoordinates(new List<GeoCoordinate> { step.Start, step.End }),
                    DistanceKm = seaDistance
                });

                totalDistance += seaDistance;
                totalDuration += EstimateFerryMinutes(seaDistance);
            }
        }

        return RouteComputationResult.CreateSuccess(segments, totalDistance, totalDuration);
    }

    private static List<RoutePlanStep> BuildPlan(Landmass fromMass, Landmass toMass, GeoCoordinate from, GeoCoordinate to)
    {
        var plan = new List<RoutePlanStep>();

        if (fromMass == toMass)
        {
            plan.Add(new RoutePlanStep(RouteStepType.Land, from, to));
            return plan;
        }

        var currentCoord = from;
        var currentMass = fromMass;

        if (currentMass != Landmass.Mainland)
        {
            if (!FerryConnections.TryGetValue(currentMass, out var connection))
            {
                return plan;
            }

            plan.Add(new RoutePlanStep(RouteStepType.Land, currentCoord, connection.IslandPort));
            plan.Add(new RoutePlanStep(RouteStepType.Sea, connection.IslandPort, connection.MainlandPort));

            currentCoord = connection.MainlandPort;
            currentMass = Landmass.Mainland;
        }

        if (toMass == Landmass.Mainland)
        {
            plan.Add(new RoutePlanStep(RouteStepType.Land, currentCoord, to));
        }
        else
        {
            if (!FerryConnections.TryGetValue(toMass, out var destinationConnection))
            {
                return plan;
            }

            plan.Add(new RoutePlanStep(RouteStepType.Land, currentCoord, destinationConnection.MainlandPort));
            plan.Add(new RoutePlanStep(RouteStepType.Sea, destinationConnection.MainlandPort, destinationConnection.IslandPort));
            plan.Add(new RoutePlanStep(RouteStepType.Land, destinationConnection.IslandPort, to));
        }

        return plan;
    }

    private static List<double[]> ConvertCoordinates(List<GeoCoordinate> coords) =>
        coords.Select(c => new[] { c.Latitude, c.Longitude }).ToList();

    private static double GeoDistance(GeoCoordinate a, GeoCoordinate b)
    {
        const double R = 6371d;
        var dLat = DegreesToRadians(b.Latitude - a.Latitude);
        var dLon = DegreesToRadians(b.Longitude - a.Longitude);
        var lat1 = DegreesToRadians(a.Latitude);
        var lat2 = DegreesToRadians(b.Latitude);

        var sinLat = Math.Sin(dLat / 2);
        var sinLon = Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(sinLat * sinLat + Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon),
            Math.Sqrt(1 - sinLat * sinLat - Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon));
        return R * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private static double EstimateFerryMinutes(double distanceKm)
    {
        const double ferrySpeedKmH = 28d;
        return (distanceKm / ferrySpeedKmH) * 60d;
    }

    private static Landmass DetermineLandmass(Location location)
    {
        var lat = location.Latitude;
        var lon = location.Longitude;
        var name = location.Name.ToLowerInvariant();

        if (name.Contains("isla mujeres") || (lat >= 21.1 && lat <= 21.35 && lon <= -86.70))
        {
            return Landmass.IslaMujeres;
        }

        if (name.Contains("cozumel") || (lat >= 20.4 && lat <= 20.6 && lon <= -86.8))
        {
            return Landmass.Cozumel;
        }

        if (name.Contains("contoy"))
        {
            return Landmass.IslaContoy;
        }

        return Landmass.Mainland;
    }
}

public record FerryConnection(Landmass Island, GeoCoordinate IslandPort, GeoCoordinate MainlandPort);

public enum Landmass
{
    Mainland,
    IslaMujeres,
    Cozumel,
    IslaContoy
}

public enum RouteStepType
{
    Land,
    Sea
}

public record RoutePlanStep(RouteStepType Type, GeoCoordinate Start, GeoCoordinate End);

public class RouteComputationResult
{
    public bool Success { get; private set; }
    public string? Message { get; private set; }
    public double DistanceKm { get; private set; }
    public double DurationMinutes { get; private set; }
    public List<RouteSegmentDto> Segments { get; private set; } = new();

    public static RouteComputationResult Failure(string message) => new()
    {
        Success = false,
        Message = message
    };

    public static RouteComputationResult CreateSuccess(List<RouteSegmentDto> segments, double distanceKm, double durationMinutes) => new()
    {
        Success = true,
        Segments = segments,
        DistanceKm = distanceKm,
        DurationMinutes = durationMinutes
    };
}
