using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://localhost:5042");

        builder.Services.AddSingleton<DataService>();
        builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:5174")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();
        app.UseCors("FrontendPolicy");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/hotels/search", async (HttpRequest request, IEnumerable<IHotelProvider> providers, DataService dataService) =>
        {
            if (!request.Query.TryGetValue("destination", out var destinationValues) || string.IsNullOrWhiteSpace(destinationValues.ToString()))
                return Results.BadRequest(new { Error = "Missing required parameter 'destination'." });
            if (!request.Query.TryGetValue("checkIn", out var checkInValues) || string.IsNullOrWhiteSpace(checkInValues.ToString()))
                return Results.BadRequest(new { Error = "Missing required parameter 'checkIn'." });
            if (!request.Query.TryGetValue("checkOut", out var checkOutValues) || string.IsNullOrWhiteSpace(checkOutValues.ToString()))
                return Results.BadRequest(new { Error = "Missing required parameter 'checkOut'." });

            var destination = destinationValues.ToString();
            var checkInValue = checkInValues.ToString();
            var checkOutValue = checkOutValues.ToString();

            if (!DateOnly.TryParse(checkInValue, out var checkIn))
                return Results.BadRequest(new { Error = "Invalid checkIn date." });
            if (!DateOnly.TryParse(checkOutValue, out var checkOut))
                return Results.BadRequest(new { Error = "Invalid checkOut date." });
            if (checkOut <= checkIn)
                return Results.BadRequest(new { Error = "checkOut must be later than checkIn." });

            RoomType? roomType = null;
            if (request.Query.TryGetValue("roomType", out var roomTypeValue) && !string.IsNullOrWhiteSpace(roomTypeValue))
            {
                if (!Enum.TryParse<RoomType>(roomTypeValue, true, out var parsedRoomType))
                    return Results.BadRequest(new { Error = "Invalid roomType value." });
                roomType = parsedRoomType;
            }

            if (!dataService.IsKnownCity(destination))
            {
                return Results.BadRequest(new { Error = "Unknown destination city." });
            }

            var results = await Task.WhenAll(providers.Select(p => p.SearchAsync(destination, checkIn, checkOut, roomType)));
            var hotels = results.SelectMany(x => x).Where(h => h.Available).OrderBy(h => h.TotalPrice);

            return Results.Ok(hotels);
        })
        .WithName("SearchHotels");

        app.MapPost("/hotels/reserve", async (ReservationRequest request, DataService dataService, IEnumerable<IHotelProvider> providers) =>
        {
            if (string.IsNullOrWhiteSpace(request.Destination) || string.IsNullOrWhiteSpace(request.CheckIn) || string.IsNullOrWhiteSpace(request.CheckOut) || string.IsNullOrWhiteSpace(request.HotelId) || string.IsNullOrWhiteSpace(request.GuestName) || string.IsNullOrWhiteSpace(request.DocumentNumber))
                return Results.BadRequest(new { Error = "One or more required reservation fields are missing." });

            if (!DateOnly.TryParse(request.CheckIn, out var checkIn))
                return Results.BadRequest(new { Error = "Invalid checkIn date." });
            if (!DateOnly.TryParse(request.CheckOut, out var checkOut))
                return Results.BadRequest(new { Error = "Invalid checkOut date." });
            if (checkOut <= checkIn)
                return Results.BadRequest(new { Error = "checkOut must be later than checkIn." });

            if (!dataService.IsKnownCity(request.Destination))
                return Results.BadRequest(new { Error = "Unknown destination city." });

            var isDomestic = dataService.IsDomestic(request.Destination);
            if (isDomestic && request.DocumentType == DocumentType.Passport)
                return Results.UnprocessableEntity(new { Error = "Domestic destinations require NationalId." });
            if (!isDomestic && request.DocumentType != DocumentType.Passport)
                return Results.UnprocessableEntity(new { Error = "International destinations require Passport." });

            var searchResults = await Task.WhenAll(providers.Select(p => p.SearchAsync(request.Destination, checkIn, checkOut, request.RoomType)));
            var hotel = searchResults.SelectMany(r => r).FirstOrDefault(h => string.Equals(h.HotelId, request.HotelId, StringComparison.OrdinalIgnoreCase));
            if (hotel is null || !hotel.Available)
                return Results.BadRequest(new { Error = "Hotel not found or unavailable." });

            var reference = Guid.NewGuid().ToString("N");
            var reservation = new ReservationRecord(
                reference,
                hotel.Provider,
                hotel.HotelId,
                hotel.Name,
                request.Destination,
                checkIn,
                checkOut,
                request.GuestName,
                request.DocumentType,
                request.DocumentNumber,
                hotel.RoomType,
                hotel.TotalPrice,
                hotel.CancellationPolicy
            );

            dataService.AddReservation(reservation);

            return Results.Created($"/hotels/reservation/{reference}", new ReservationResponse(
                reference,
                reservation.Provider,
                reservation.HotelId,
                reservation.HotelName,
                reservation.Destination,
                reservation.CheckIn.ToString("yyyy-MM-dd"),
                reservation.CheckOut.ToString("yyyy-MM-dd"),
                reservation.GuestName,
                reservation.DocumentType,
                reservation.DocumentNumber,
                reservation.RoomType,
                reservation.TotalPrice,
                reservation.CancellationPolicy
            ));
        })
        .WithName("CreateReservation");

        app.MapGet("/hotels/reservation/{reference}", (string reference, DataService dataService) =>
        {
            var reservation = dataService.GetReservation(reference);
            return reservation is null
                ? Results.NotFound(new { Error = "Reservation not found." })
                : Results.Ok(new ReservationResponse(
                    reservation.Reference,
                    reservation.Provider,
                    reservation.HotelId,
                    reservation.HotelName,
                    reservation.Destination,
                    reservation.CheckIn.ToString("yyyy-MM-dd"),
                    reservation.CheckOut.ToString("yyyy-MM-dd"),
                    reservation.GuestName,
                    reservation.DocumentType,
                    reservation.DocumentNumber,
                    reservation.RoomType,
                    reservation.TotalPrice,
                    reservation.CancellationPolicy
                ));
        })
        .WithName("GetReservation");

        app.Run();
    }
}
