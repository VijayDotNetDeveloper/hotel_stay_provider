using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelStay.Api.Models;

public sealed record SearchRequest(
    [Required] string? Destination,
    [Required] DateOnly? CheckIn,
    [Required] DateOnly? CheckOut,
    RoomType? RoomType) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckIn.HasValue && CheckOut.HasValue && CheckOut.Value <= CheckIn.Value)
        {
            yield return new ValidationResult("checkOut must be later than checkIn.", new[] { nameof(CheckOut) });
        }
    }
}
