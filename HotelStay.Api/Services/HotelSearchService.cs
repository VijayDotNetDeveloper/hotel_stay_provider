using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IHotelSearchService
{
    Task<IEnumerable<HotelDto>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType);
}

public sealed class HotelSearchService : IHotelSearchService
{
    private readonly IEnumerable<IHotelProvider> _providers;
    private readonly DataService _dataService;

    public HotelSearchService(IEnumerable<IHotelProvider> providers, DataService dataService)
    {
        _providers = providers;
        _dataService = dataService;
    }

    public async Task<IEnumerable<HotelDto>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType)
    {
        ValidateSearchArguments(destination, checkIn, checkOut);

        var results = await Task.WhenAll(_providers.Select(p => p.SearchAsync(destination, checkIn, checkOut, roomType)));
        return results
            .SelectMany(x => x)
            .Where(h => h.Available)
            .OrderBy(h => h.TotalPrice)
            .ToArray();
    }

    private void ValidateSearchArguments(string? destination, DateOnly checkIn, DateOnly checkOut)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new RequestValidationException("Missing required parameter 'destination'.", nameof(destination));
        }

        if (!_dataService.IsKnownCity(destination!))
        {
            throw new RequestValidationException("Unknown destination city.", nameof(destination));
        }

        if (checkOut <= checkIn)
        {
            throw new RequestValidationException("checkOut must be later than checkIn.", nameof(checkOut));
        }
    }
}
