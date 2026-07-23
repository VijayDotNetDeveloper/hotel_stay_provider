using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using HotelStay.Api;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelStay.Tests;

public class HotelStayTests
{
    private WebApplicationFactory<Program>? _factory;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TearDown]
    public void TearDown()
    {
        _factory?.Dispose();
    }

    [Test]
    public async Task NormaliseBudgetNestsSnakeCaseData()
    {
        var provider = new BudgetNestsProvider(new DataService());
        var hotels = await provider.SearchAsync("New York", DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-04"), RoomType.Deluxe);
        var hotel = hotels.Single();

        Assert.That(hotel.Provider, Is.EqualTo("BudgetNests"));
        Assert.That(hotel.RoomType, Is.EqualTo(RoomType.Deluxe));
        Assert.That(hotel.CancellationPolicy, Is.EqualTo(CancellationPolicy.NonRefundable).Or.EqualTo(CancellationPolicy.Flexible).Or.EqualTo(CancellationPolicy.FreeCancellation));
        Assert.That(hotel.Available, Is.True);
    }

    [Test]
    public async Task SearchFiltersUnavailableBudgetNestsRooms()
    {
        var dataService = new DataService();
        var provider = new BudgetNestsProvider(dataService);
        var hotels = await provider.SearchAsync("Tokyo", DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-03"), null);

        Assert.That(hotels, Is.Not.Empty);
        Assert.That(hotels.All(h => h.Available), Is.True);
    }

    [Test]
    public async Task SearchEndpointReturnsBadRequestForMissingRequiredParameters()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/hotels/search?destination=Paris&checkIn=2026-08-01");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task SearchEndpointReturnsBadRequestForInvalidDates()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/hotels/search?destination=Paris&checkIn=not-a-date&checkOut=2026-08-05");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ReserveEndpointReturnsUnprocessableForDocumentMismatch()
    {
        var client = _factory!.CreateClient();
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

        var response = await client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)422));
    }

    private static readonly System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    [Test]
    public async Task ReserveEndpointCreatesReservationForValidRequest()
    {
        var client = _factory!.CreateClient();
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

        var response = await client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var reservation = await response.Content.ReadFromJsonAsync<ReservationResponse>(_jsonSerializerOptions);
        Assert.That(reservation, Is.Not.Null);
        Assert.That(reservation!.Reference, Is.Not.Null.And.Not.Empty);
        Assert.That(reservation.Provider, Is.EqualTo("PremierStays"));
    }

    [Test]
    public async Task SearchEndpointReturnsResultsForValidQuery()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/hotels/search?destination=Paris&checkIn=2026-08-01&checkOut=2026-08-05");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var hotels = await response.Content.ReadFromJsonAsync<List<HotelDto>>(_jsonSerializerOptions);
        Assert.That(hotels, Is.Not.Null);
        Assert.That(hotels, Is.Not.Empty);
        Assert.That(hotels!.Any(h => h.HotelId.StartsWith("PREM-") || h.HotelId.StartsWith("BUD-")), Is.True);
    }

    [Test]
    public async Task SearchEndpointReturnsBadRequestForUnknownDestination()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/hotels/search?destination=UnknownCity&checkIn=2026-08-01&checkOut=2026-08-05");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ReserveEndpointReturnsBadRequestForUnknownHotel()
    {
        var client = _factory!.CreateClient();
        var request = new ReservationRequest(
            "Paris",
            "2026-08-01",
            "2026-08-05",
            "UNKNOWN-001",
            RoomType.Standard,
            "Alice Example",
            DocumentType.Passport,
            "P1234567"
        );

        var response = await client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ReservationEndpointReturnsNotFoundForMissingReference()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/hotels/reservation/not-a-real-reference");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReservationEndpointReturnsReservationByReference()
    {
        var client = _factory!.CreateClient();
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

        var createResponse = await client.PostAsJsonAsync("/hotels/reserve", request);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdReservation = await createResponse.Content.ReadFromJsonAsync<ReservationResponse>(_jsonSerializerOptions);
        Assert.That(createdReservation, Is.Not.Null);

        var reference = createdReservation!.Reference;
        var getResponse = await client.GetAsync($"/hotels/reservation/{reference}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetchedReservation = await getResponse.Content.ReadFromJsonAsync<ReservationResponse>(_jsonSerializerOptions);
        Assert.That(fetchedReservation, Is.Not.Null);
        Assert.That(fetchedReservation!.Reference, Is.EqualTo(reference));
        Assert.That(fetchedReservation.HotelId, Is.EqualTo("PREM-103"));
        Assert.That(fetchedReservation.Provider, Is.EqualTo("PremierStays"));
        Assert.That(fetchedReservation.GuestName, Is.EqualTo("Alice Example"));
    }
}
