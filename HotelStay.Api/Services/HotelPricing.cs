namespace HotelStay.Api.Services;

internal static class HotelPricing
{
    public static decimal CalculateTotal(decimal ratePerNight, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        return ratePerNight * nights;
    }
}
