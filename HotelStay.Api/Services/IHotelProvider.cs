using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IHotelProvider
{
    string ProviderName { get; }
    Task<IEnumerable<HotelDto>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType);
}
