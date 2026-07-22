using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Tests;

public class ReservationServiceTests
{
    [Test]
    public async Task CreateReservationAsync_ValidRequest_ReturnsReservationResponse()
    {
        var dataService = new DataService();
        var providers = new IHotelProvider[]
        {
            new PremierStaysProvider(dataService),
            new BudgetNestsProvider(dataService)
        };

        var searchService = new HotelSearchService(providers, dataService);
        var reservationRepository = new InMemoryReservationRepository();
        var reservationService = new ReservationService(searchService, reservationRepository, dataService);

        var request = new ReservationRequest(
            "Paris",
            "2026-08-01",
            "2026-08-05",
            "PREM-103",
            RoomType.Standard,
            "Alice Example",
            DocumentType.Passport,
            "P1234567"
        );

        var response = await reservationService.CreateReservationAsync(request);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Reference, Is.Not.Null.And.Not.Empty);
        Assert.That(response.Provider, Is.EqualTo("PremierStays"));
        Assert.That(response.HotelId, Is.EqualTo("PREM-103"));
    }

    [Test]
    public void CreateReservationAsync_InvalidDocumentType_ThrowsValidationException()
    {
        var dataService = new DataService();
        var providers = new IHotelProvider[]
        {
            new PremierStaysProvider(dataService),
            new BudgetNestsProvider(dataService)
        };

        var searchService = new HotelSearchService(providers, dataService);
        var reservationRepository = new InMemoryReservationRepository();
        var reservationService = new ReservationService(searchService, reservationRepository, dataService);

        var request = new ReservationRequest(
            "Paris",
            "2026-08-01",
            "2026-08-05",
            "PREM-103",
            RoomType.Standard,
            "Alice Example",
            DocumentType.NationalId,
            "12345"
        );

        var exception = Assert.ThrowsAsync<RequestValidationException>(async () =>
            await reservationService.CreateReservationAsync(request));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Errors, Contains.Key("DocumentType"));
    }
}
