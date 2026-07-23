using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Tests;

public class BudgetNestsProviderTests
{
    [Test]
    public async Task SearchAsync_ExcludesDeterministicallyUnavailableBudgetNestsRooms()
    {
        var dataService = new DataService();
        var provider = new BudgetNestsProvider(dataService);

        var checkIn = DateOnly.Parse("2026-08-04");
        var checkOut = DateOnly.Parse("2026-08-06");
        var hotels = await provider.SearchAsync("Seattle", checkIn, checkOut, null);

        Assert.That(hotels, Is.Not.Empty);
        Assert.That(hotels.All(h => h.Available), Is.True);
    }
}
