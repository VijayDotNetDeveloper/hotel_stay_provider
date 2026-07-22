using System.Text.Json.Serialization;

namespace HotelStay.Api.Services;

public sealed record BudgetNestsSnakeCaseRoom(
    [property: JsonPropertyName("hotel_id")] string HotelId,
    [property: JsonPropertyName("hotel_name")] string HotelName,
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("room_type")] string RoomType,
    [property: JsonPropertyName("rate_per_night")] decimal RatePerNight,
    [property: JsonPropertyName("cancellation_policy")] string CancellationPolicy,
    [property: JsonPropertyName("available")] bool Available);
