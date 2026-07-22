using HotelStay.Api.Controllers;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Tests;

public class HotelsControllerTests
{
    [Test]
    public async Task SearchHotels_ReturnsOk_WhenRequestIsValid()
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
        var controller = new HotelsController(searchService, reservationService);

        var request = new SearchRequest("Paris", DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-05"), null);

        var result = await controller.SearchHotels(request);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<IEnumerable<HotelDto>>());
    }

    [Test]
    public void SearchHotels_ReturnsBadRequest_WhenModelStateInvalid()
    {
        var dataService = new DataService();
        var providers = new IHotelProvider[] { new PremierStaysProvider(dataService), new BudgetNestsProvider(dataService) };
        var searchService = new HotelSearchService(providers, dataService);
        var reservationRepository = new InMemoryReservationRepository();
        var reservationService = new ReservationService(searchService, reservationRepository, dataService);
        var controller = new HotelsController(searchService, reservationService);
        controller.ModelState.AddModelError("destination", "The destination field is required.");

        var request = new SearchRequest(null!, DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-05"), null);

        var result = controller.SearchHotels(request).Result;

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
