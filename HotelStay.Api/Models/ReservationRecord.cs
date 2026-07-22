namespace HotelStay.Api.Models;

public sealed record ReservationRecord(
    string Reference,
    string Provider,
    string HotelId,
    string HotelName,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    RoomType RoomType,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy
);
