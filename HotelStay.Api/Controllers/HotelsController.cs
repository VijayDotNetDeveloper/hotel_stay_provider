using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HotelStay.Api.Controllers;

[ApiController]
[Route("hotels")]
public class HotelsController : ControllerBase
{
    private readonly IHotelSearchService _searchService;
    private readonly IReservationService _reservationService;

    public HotelsController(IHotelSearchService searchService, IReservationService reservationService)
    {
        _searchService = searchService;
        _reservationService = reservationService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchHotels([FromQuery] SearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var hotels = await _searchService.SearchAsync(request.Destination!, request.CheckIn!.Value, request.CheckOut!.Value, request.RoomType);
            return Ok(hotels);
        }
        catch (RequestValidationException validationException)
        {
            var details = new ValidationProblemDetails(validationException.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid search request"
            };
            return BadRequest(details);
        }
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationRequest request)
    {
        try
        {
            var reservation = await _reservationService.CreateReservationAsync(request);
            return CreatedAtAction(nameof(GetReservation), new { reference = reservation.Reference }, reservation);
        }
        catch (RequestValidationException validationException)
        {
            var details = new ValidationProblemDetails(validationException.Errors)
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Request validation failed"
            };
            return UnprocessableEntity(details);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = invalidOperationException.Message
            });
        }
    }

    [HttpGet("reservation/{reference}")]
    public IActionResult GetReservation(string reference)
    {
        var reservation = _reservationService.GetReservation(reference);
        if (reservation is null)
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status404NotFound, "Reservation not found."));

        return Ok(reservation);
    }

    private IActionResult BadRequestValidation(string field, string message)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(field, message);
        return ValidationProblem(modelState);
    }
}
