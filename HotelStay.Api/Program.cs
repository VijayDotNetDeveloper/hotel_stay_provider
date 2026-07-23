using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var serverUrl = builder.Configuration.GetValue<string>("Api:Urls");
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            builder.WebHost.UseUrls(serverUrl);
        }

        builder.Services.AddSingleton<DataService>();
        builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
        builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
        builder.Services.AddSingleton<IHotelSearchService, HotelSearchService>();
        builder.Services.AddSingleton<IReservationService, ReservationService>();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                policy.AllowAnyHeader()
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

        app.UseHttpsRedirection();

        app.MapGet("/hotels/search", async (
            [FromQuery] string? destination,
            [FromQuery] string? checkIn,
            [FromQuery] string? checkOut,
            [FromQuery] RoomType? roomType,
            IHotelSearchService searchService) =>
        {
            var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var checkInDate = default(DateOnly?);
            var checkOutDate = default(DateOnly?);

            if (string.IsNullOrWhiteSpace(destination))
            {
                errors[nameof(SearchRequest.Destination)] = new[] { "Destination is required." };
            }

            if (string.IsNullOrWhiteSpace(checkIn))
            {
                errors[nameof(SearchRequest.CheckIn)] = new[] { "CheckIn is required." };
            }
            else if (!DateOnly.TryParse(checkIn, out var parsedCheckIn))
            {
                errors[nameof(SearchRequest.CheckIn)] = new[] { "Invalid checkIn date." };
            }
            else
            {
                checkInDate = parsedCheckIn;
            }

            if (string.IsNullOrWhiteSpace(checkOut))
            {
                errors[nameof(SearchRequest.CheckOut)] = new[] { "CheckOut is required." };
            }
            else if (!DateOnly.TryParse(checkOut, out var parsedCheckOut))
            {
                errors[nameof(SearchRequest.CheckOut)] = new[] { "Invalid checkOut date." };
            }
            else
            {
                checkOutDate = parsedCheckOut;
            }

            if (checkInDate.HasValue && checkOutDate.HasValue && checkOutDate.Value <= checkInDate.Value)
            {
                errors[nameof(SearchRequest.CheckOut)] = new[] { "checkOut must be later than checkIn." };
            }

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            try
            {
                var hotels = await searchService.SearchAsync(destination!, checkInDate.Value, checkOutDate.Value, roomType);
                return Results.Ok(hotels);
            }
            catch (RequestValidationException validationException)
            {
                var details = new ValidationProblemDetails(validationException.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid search request"
                };
                return Results.BadRequest(details);
            }
        });

        app.MapPost("/hotels/reserve", async (ReservationRequest request, IReservationService reservationService) =>
        {
            try
            {
                var reservation = await reservationService.CreateReservationAsync(request);
                return Results.Created($"/hotels/reservation/{reservation.Reference}", reservation);
            }
            catch (RequestValidationException validationException)
            {
                var details = new ValidationProblemDetails(validationException.Errors)
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Request validation failed"
                };
                return Results.UnprocessableEntity(details);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = invalidOperationException.Message
                });
            }
        });

        app.MapGet("/hotels/reservation/{reference}", (string reference, IReservationService reservationService) =>
        {
            var reservation = reservationService.GetReservation(reference);
            if (reservation is null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Reservation not found."
                });
            }

            return Results.Ok(reservation);
        });

        app.MapGet("/api/v1/health", () => Results.Ok(new { status = "Healthy", uptime = "available" }));

        app.Run();
    }
}
