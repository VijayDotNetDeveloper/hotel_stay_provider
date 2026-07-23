using System.Net;
using System.Net.Http.Json;
using HotelStay.Api;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelStay.Tests;

public class ProgramApiTests
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
    public async Task SearchEndpoint_ShouldReturnBadRequestForMissingRequiredParameters()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/hotels/search?destination=Paris&checkIn=2026-08-01");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
