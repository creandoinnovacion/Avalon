namespace TransportQuote.Models;

public class OpenRouteServiceOptions
{
    public string BaseUrl { get; set; } = "https://api.openrouteservice.org";
    public string ApiKey { get; set; } = string.Empty;
}
