using Microsoft.AspNetCore.Mvc.RazorPages;
using TransportQuote.Models;
using TransportQuote.Services;

namespace TransportQuote.Pages;

public class IndexModel : PageModel
{
    private readonly LocationService _locationService;

    public IndexModel(LocationService locationService)
    {
        _locationService = locationService;
    }

    public IEnumerable<Location> Locations { get; private set; } = new List<Location>();

    public void OnGet()
    {
        Locations = _locationService.GetLocations();
    }
}
