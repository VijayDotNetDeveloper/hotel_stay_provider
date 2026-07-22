using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IReservationService
{
    Task<ReservationResponse> CreateReservationAsync(ReservationRequest request);
    ReservationResponse? GetReservation(string reference);
}

public sealed class ReservationService : IReservationService
{
    private readonly IHotelSearchService _searchService;
    private readonly IReservationRepository _reservationRepository;
    private readonly DataService _dataService;

    public ReservationService(
        IHotelSearchService searchService,
        IReservationRepository reservationRepository,
        DataService dataService)
    {
        _searchService = searchService;
        _reservationRepository = reservationRepository;
        _dataService = dataService;
    }

    public async Task<ReservationResponse> CreateReservationAsync(ReservationRequest request)
    {
        if (request is null)
        {
            throw new RequestValidationException("Reservation request body is required.", nameof(request));
        }

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            errors[nameof(request.Destination)] = new[] { "Destination is required." };
        }

        if (string.IsNullOrWhiteSpace(request.CheckIn))
        {
            errors[nameof(request.CheckIn)] = new[] { "CheckIn is required." };
        }

        if (string.IsNullOrWhiteSpace(request.CheckOut))
        {
            errors[nameof(request.CheckOut)] = new[] { "CheckOut is required." };
        }

        if (string.IsNullOrWhiteSpace(request.HotelId))
        {
            errors[nameof(request.HotelId)] = new[] { "HotelId is required." };
        }

        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            errors[nameof(request.GuestName)] = new[] { "GuestName is required." };
        }

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors[nameof(request.DocumentNumber)] = new[] { "DocumentNumber is required." };
        }

        if (errors.Any())
        {
            throw new RequestValidationException(errors);
        }

        if (!DateOnly.TryParse(request.CheckIn, out var parsedCheckIn))
        {
            throw new RequestValidationException("Invalid checkIn date.", nameof(request.CheckIn));
        }

        if (!DateOnly.TryParse(request.CheckOut, out var parsedCheckOut))
        {
            throw new RequestValidationException("Invalid checkOut date.", nameof(request.CheckOut));
        }

        if (parsedCheckOut <= parsedCheckIn)
        {
            throw new RequestValidationException("checkOut must be later than checkIn.", nameof(request.CheckOut));
        }

        if (!_dataService.IsKnownCity(request.Destination))
        {
            throw new RequestValidationException("Unknown destination city.", nameof(request.Destination));
        }

        var isDomestic = _dataService.IsDomestic(request.Destination);
        if (isDomestic && request.DocumentType == DocumentType.Passport)
        {
            throw new RequestValidationException("Domestic destinations require NationalId.", nameof(request.DocumentType));
        }

        if (!isDomestic && request.DocumentType != DocumentType.Passport)
        {
            throw new RequestValidationException("International destinations require Passport.", nameof(request.DocumentType));
        }

        var hotels = await _searchService.SearchAsync(request.Destination, parsedCheckIn, parsedCheckOut, request.RoomType);
        var hotel = hotels.FirstOrDefault(h => string.Equals(h.HotelId, request.HotelId, StringComparison.OrdinalIgnoreCase));

        if (hotel is null)
        {
            throw new InvalidOperationException("Hotel not found or unavailable.");
        }

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

        _reservationRepository.Add(reservation);

        return new ReservationResponse(
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
    }

    public ReservationResponse? GetReservation(string reference)
    {
        var reservation = _reservationRepository.Get(reference);
        if (reservation is null)
        {
            return null;
        }

        return new ReservationResponse(
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
    }
}
