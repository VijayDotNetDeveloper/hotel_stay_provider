using System.Collections.Generic;

namespace HotelStay.Api.Services;

public sealed class RequestValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public RequestValidationException(string message, string field)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [field] = new[] { message }
        };
    }

    public RequestValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
