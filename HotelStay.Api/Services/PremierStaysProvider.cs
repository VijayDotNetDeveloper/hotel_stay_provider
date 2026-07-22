using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public sealed class PremierStaysProvider : IHotelProvider
{
    private readonly DataService _dataService;

    public PremierStaysProvider(DataService dataService)
    {
        _dataService = dataService;
    }

    public string ProviderName => "PremierStays";

    public Task<IEnumerable<HotelDto>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType)
    {
        var raw = _dataService.PremierRooms
            .Where(item => string.Equals(item.Destination, destination, StringComparison.OrdinalIgnoreCase))
            .Where(item => roomType == null || item.RoomType == roomType.Value)
            .Select(item => new HotelDto(
                ProviderName,
                item.HotelId,
                item.HotelName,
                item.RoomType,
                item.RatePerNight,
                CalculateTotal(item.RatePerNight, checkIn, checkOut),
                item.CancellationPolicy,
                item.Amenities,
                item.StarRating,
                true
            ));

        return Task.FromResult(raw);
    }

    private static decimal CalculateTotal(decimal ratePerNight, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        return ratePerNight * nights;
    }
}
