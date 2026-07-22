namespace HotelStay.Api.Models;

public sealed record ReservationRequest(
    string Destination,
    string CheckIn,
    string CheckOut,
    string HotelId,
    RoomType RoomType,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber
);
