using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Tests;

public class HotelSearchServiceTests
{
    [Test]
    public async Task SearchAsync_ReturnsAvailableHotelsOrderedByTotalPrice()
    {
        var dataService = new DataService();
        var providers = new IHotelProvider[]
        {
            new PremierStaysProvider(dataService),
            new BudgetNestsProvider(dataService)
        };

        var searchService = new HotelSearchService(providers, dataService);
        var hotels = await searchService.SearchAsync("Paris", DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-05"), null);

        Assert.That(hotels, Is.Not.Empty);
        Assert.That(hotels.All(h => h.Available), Is.True);
        Assert.That(hotels, Is.Ordered.By("TotalPrice"));
    }

    [Test]
    public void SearchAsync_UnknownCity_ThrowsValidationException()
    {
        var dataService = new DataService();
        var providers = new IHotelProvider[]
        {
            new PremierStaysProvider(dataService),
            new BudgetNestsProvider(dataService)
        };

        var searchService = new HotelSearchService(providers, dataService);

        var exception = Assert.ThrowsAsync<RequestValidationException>(async () =>
            await searchService.SearchAsync("UnknownCity", DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-05"), null));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Errors, Contains.Key("destination"));
    }
}
