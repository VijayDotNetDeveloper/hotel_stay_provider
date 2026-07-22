namespace HotelStay.Api.Models;

public sealed record HotelDto(
    string Provider,
    string HotelId,
    string Name,
    RoomType RoomType,
    decimal RatePerNight,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    IReadOnlyList<string>? Amenities,
    int? StarRating,
    bool Available
);
