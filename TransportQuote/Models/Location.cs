namespace TransportQuote.Models;

public record Location(
    string Id,
    string Name,
    string Type, // "Beach", "Hotel", "Attraction"
    double Latitude,
    double Longitude,
    string Description,
    string ImageUrl
);
