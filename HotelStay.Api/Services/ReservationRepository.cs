using System.Collections.Concurrent;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IReservationRepository
{
    void Add(ReservationRecord record);
    ReservationRecord? Get(string reference);
}

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<string, ReservationRecord> _reservations = new(StringComparer.OrdinalIgnoreCase);

    public void Add(ReservationRecord record)
    {
        _reservations[record.Reference] = record;
    }

    public ReservationRecord? Get(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return null;
        }

        return _reservations.GetValueOrDefault(reference);
    }
}
