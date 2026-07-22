using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Controllers;

[ApiController]
[Route("hotels")]
public class HotelsController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly IEnumerable<IHotelProvider> _providers;

    public HotelsController(DataService dataService, IEnumerable<IHotelProvider> providers)
    {
        _dataService = dataService;
        _providers = providers;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchHotels(
        [FromQuery] string? destination,
        [FromQuery] string? checkIn,
        [FromQuery] string? checkOut,
        [FromQuery] string? roomType)
    {
        if (string.IsNullOrWhiteSpace(destination))
            return BadRequest(new { Error = "Missing required parameter 'destination'." });

        if (string.IsNullOrWhiteSpace(checkIn))
            return BadRequest(new { Error = "Missing required parameter 'checkIn'." });

        if (string.IsNullOrWhiteSpace(checkOut))
            return BadRequest(new { Error = "Missing required parameter 'checkOut'." });

        if (!DateOnly.TryParse(checkIn, out var parsedCheckIn))
            return BadRequest(new { Error = "Invalid checkIn date." });

        if (!DateOnly.TryParse(checkOut, out var parsedCheckOut))
            return BadRequest(new { Error = "Invalid checkOut date." });

        if (parsedCheckOut <= parsedCheckIn)
            return BadRequest(new { Error = "checkOut must be later than checkIn." });

        RoomType? parsedRoomType = null;
        if (!string.IsNullOrWhiteSpace(roomType))
        {
            if (!Enum.TryParse<RoomType>(roomType, true, out var tempRoomType))
                return BadRequest(new { Error = "Invalid roomType value." });

            parsedRoomType = tempRoomType;
        }

        if (!_dataService.IsKnownCity(destination))
            return BadRequest(new { Error = "Unknown destination city." });

        var results = await Task.WhenAll(_providers.Select(p => p.SearchAsync(destination, parsedCheckIn, parsedCheckOut, parsedRoomType)));
        var hotels = results.SelectMany(x => x).Where(h => h.Available).OrderBy(h => h.TotalPrice);

        return Ok(hotels);
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationRequest request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Destination) ||
            string.IsNullOrWhiteSpace(request.CheckIn) ||
            string.IsNullOrWhiteSpace(request.CheckOut) ||
            string.IsNullOrWhiteSpace(request.HotelId) ||
            string.IsNullOrWhiteSpace(request.GuestName) ||
            string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            return BadRequest(new { Error = "One or more required reservation fields are missing." });
        }

        if (!DateOnly.TryParse(request.CheckIn, out var parsedCheckIn))
            return BadRequest(new { Error = "Invalid checkIn date." });

        if (!DateOnly.TryParse(request.CheckOut, out var parsedCheckOut))
            return BadRequest(new { Error = "Invalid checkOut date." });

        if (parsedCheckOut <= parsedCheckIn)
            return BadRequest(new { Error = "checkOut must be later than checkIn." });

        if (!_dataService.IsKnownCity(request.Destination))
            return BadRequest(new { Error = "Unknown destination city." });

        var isDomestic = _dataService.IsDomestic(request.Destination);
        if (isDomestic && request.DocumentType == DocumentType.Passport)
            return UnprocessableEntity(new { Error = "Domestic destinations require NationalId." });
        if (!isDomestic && request.DocumentType != DocumentType.Passport)
            return UnprocessableEntity(new { Error = "International destinations require Passport." });

        var searchResults = await Task.WhenAll(_providers.Select(p => p.SearchAsync(request.Destination, parsedCheckIn, parsedCheckOut, request.RoomType)));
        var hotel = searchResults.SelectMany(r => r).FirstOrDefault(h => string.Equals(h.HotelId, request.HotelId, StringComparison.OrdinalIgnoreCase));

        if (hotel is null || !hotel.Available)
            return BadRequest(new { Error = "Hotel not found or unavailable." });

        var reference = Guid.NewGuid().ToString("N");
        var reservation = new ReservationRecord(
            reference,
            hotel.Provider,
            hotel.HotelId,
            hotel.Name,
            request.Destination,
            parsedCheckIn,
            parsedCheckOut,
            request.GuestName,
            request.DocumentType,
            request.DocumentNumber,
            hotel.RoomType,
            hotel.TotalPrice,
            hotel.CancellationPolicy
        );

        _dataService.AddReservation(reservation);

        var response = new ReservationResponse(
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
        );

        return CreatedAtAction(nameof(GetReservation), new { reference }, response);
    }

    [HttpGet("reservation/{reference}")]
    public IActionResult GetReservation(string reference)
    {
        var reservation = _dataService.GetReservation(reference);
        if (reservation is null)
            return NotFound(new { Error = "Reservation not found." });

        var response = new ReservationResponse(
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
        );

        return Ok(response);
    }
}
