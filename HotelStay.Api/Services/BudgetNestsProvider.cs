using System.Text.Json;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public sealed class BudgetNestsProvider : IHotelProvider
{
    private readonly DataService _dataService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BudgetNestsProvider(DataService dataService)
    {
        _dataService = dataService;
    }

    public string ProviderName => "BudgetNests";

    public Task<IEnumerable<HotelDto>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType)
    {
        var raw = _dataService.BudgetRoomsJson
            .Select(json => JsonSerializer.Deserialize<BudgetNestsSnakeCaseRoom>(json, _jsonOptions))
            .Where(item => item is not null)
            .Select(item => item!)
            .Where(item => string.Equals(item.Destination, destination, StringComparison.OrdinalIgnoreCase))
            .Select(item => Normalize(item, checkIn, checkOut))
            .Where(item => roomType == null || item.RoomType == roomType.Value);

        return Task.FromResult(raw);
    }

    private HotelDto Normalize(BudgetNestsSnakeCaseRoom raw, DateOnly checkIn, DateOnly checkOut)
    {
        var roomType = Enum.TryParse<RoomType>(raw.RoomType, true, out var parsedRoomType)
            ? parsedRoomType
            : RoomType.Standard;

        var totalPrice = CalculateTotal(raw.RatePerNight, checkIn, checkOut);
        var isAvailable = raw.Available && IsDeterministicallyAvailable(raw.HotelId, checkIn);

        return new HotelDto(
            ProviderName,
            raw.HotelId,
            raw.HotelName,
            roomType,
            raw.RatePerNight,
            totalPrice,
            ParseCancellationPolicy(raw.CancellationPolicy),
            null,
            null,
            isAvailable
        );
    }

    private static decimal CalculateTotal(decimal ratePerNight, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        return ratePerNight * nights;
    }

    private static CancellationPolicy ParseCancellationPolicy(string policy)
    {
        return policy.ToLowerInvariant() switch
        {
            "flexible" => CancellationPolicy.Flexible,
            "nonrefundable" => CancellationPolicy.NonRefundable,
            "limited" => CancellationPolicy.Flexible,
            _ => CancellationPolicy.NonRefundable
        };
    }

    private static bool IsDeterministicallyAvailable(string hotelId, DateOnly checkIn)
    {
        var score = hotelId.Sum(static c => c) + checkIn.DayNumber;
        return score % 3 != 0;
    }
}
