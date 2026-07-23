using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public sealed class DataService
{
    public static IReadOnlySet<string> DomesticCities { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Seattle",
        "Portland",
        "New York"
    };

    public static IReadOnlySet<string> InternationalCities { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Paris",
        "Tokyo",
        "London"
    };

    public IReadOnlyList<PremierStaysRoom> PremierRooms { get; } = new List<PremierStaysRoom>
    {
        new("PREM-101", "Premier Ocean View", "Seattle", RoomType.Suite, 329m, CancellationPolicy.FreeCancellation, new[] { "Pool", "Breakfast", "WiFi" }, 5),
        new("PREM-102", "Premier City Loft", "New York", RoomType.Deluxe, 249m, CancellationPolicy.FreeCancellation, new[] { "Gym", "WiFi" }, 4),
        new("PREM-103", "Premier Harbor Stay", "Paris", RoomType.Standard, 189m, CancellationPolicy.FreeCancellation, new[] { "WiFi", "Breakfast" }, 4),
        new("PREM-104", "Premier Luxe Suites", "Tokyo", RoomType.Suite, 399m, CancellationPolicy.NonRefundable, new[] { "Spa", "Gym", "WiFi" }, 5)
    };

    public IReadOnlyList<string> BudgetRoomsJson { get; } = new List<string>
    {
        "{\"hotel_id\":\"BUD-201\",\"hotel_name\":\"Budget Nest Downtown\",\"destination\":\"Seattle\",\"room_type\":\"Standard\",\"rate_per_night\":89.00,\"cancellation_policy\":\"Flexible\",\"available\":true}",
        "{\"hotel_id\":\"BUD-202\",\"hotel_name\":\"Budget Nest Midtown\",\"destination\":\"New York\",\"room_type\":\"Deluxe\",\"rate_per_night\":119.00,\"cancellation_policy\":\"NonRefundable\",\"available\":true}",
        "{\"hotel_id\":\"BUD-203\",\"hotel_name\":\"Budget Nest Central\",\"destination\":\"Paris\",\"room_type\":\"Standard\",\"rate_per_night\":99.00,\"cancellation_policy\":\"Flexible\",\"available\":true}",
        "{\"hotel_id\":\"BUD-204\",\"hotel_name\":\"Budget Nest Express\",\"destination\":\"Tokyo\",\"room_type\":\"Suite\",\"rate_per_night\":179.00,\"cancellation_policy\":\"Flexible\",\"available\":true}",
        "{\"hotel_id\":\"BUD-205\",\"hotel_name\":\"Budget Nest Garden\",\"destination\":\"London\",\"room_type\":\"Deluxe\",\"rate_per_night\":129.00,\"cancellation_policy\":\"Flexible\",\"available\":true}"
    };

    public bool IsDomestic(string city) => DomesticCities.Contains(city);

    public bool IsInternational(string city) => InternationalCities.Contains(city);

    public bool IsKnownCity(string city) => IsDomestic(city) || IsInternational(city);


    public record PremierStaysRoom(
        string HotelId,
        string HotelName,
        string Destination,
        RoomType RoomType,
        decimal RatePerNight,
        CancellationPolicy CancellationPolicy,
        IReadOnlyList<string> Amenities,
        int StarRating);

    public record BudgetNestsRoom(
        string HotelId,
        string HotelName,
        string Destination,
        string RoomType,
        decimal RatePerNight,
        string CancellationPolicy,
        bool Available);
}
