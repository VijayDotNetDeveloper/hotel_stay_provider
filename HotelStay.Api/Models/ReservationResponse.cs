namespace HotelStay.Api.Models;

public sealed record ReservationResponse(
    string Reference,
    string Provider,
    string HotelId,
    string HotelName,
    string Destination,
    string CheckIn,
    string CheckOut,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    RoomType RoomType,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy
);
