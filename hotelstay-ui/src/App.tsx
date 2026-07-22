import { useMemo, useState } from 'react';
import './App.css';
import type { Hotel, ReservationRequest, RoomType, DocumentType } from './types';
import { searchHotels, reserveHotel } from './services/api';

const destinations = ['Seattle', 'Portland', 'New York', 'Paris', 'Tokyo', 'London'];
const roomTypes: RoomType[] = ['Standard', 'Deluxe', 'Suite'];
const documentTypes: DocumentType[] = ['Passport', 'NationalId'];

function App() {
  const [destination, setDestination] = useState('Seattle');
  const [checkIn, setCheckIn] = useState('2026-08-01');
  const [checkOut, setCheckOut] = useState('2026-08-05');
  const [roomType, setRoomType] = useState<RoomType | ''>('');
  const [hotels, setHotels] = useState<Hotel[]>([]);
  const [selectedHotel, setSelectedHotel] = useState<Hotel | null>(null);
  const [guestName, setGuestName] = useState('');
  const [documentType, setDocumentType] = useState<DocumentType>('Passport');
  const [documentNumber, setDocumentNumber] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [confirmation, setConfirmation] = useState<null | {
    reference: string;
    provider: string;
    totalPrice: number;
    cancellationPolicy: string;
    hotelName: string;
  }>(null);

  const sortedHotels = useMemo(() => [...hotels].sort((a, b) => a.totalPrice - b.totalPrice), [hotels]);
  const canSearch = Boolean(destination && checkIn && checkOut);

  const handleSearch = async () => {
    setError('');
    setConfirmation(null);
    setSelectedHotel(null);

    if (!canSearch) {
      setError('Please complete destination and dates before searching.');
      return;
    }

    setLoading(true);
    try {
      const results = await searchHotels(destination, checkIn, checkOut, roomType || undefined);
      setHotels(results);
      if (!results.length) {
        setError('No available hotel offers found for the selected criteria.');
      }
    } catch (err) {
      setError((err as Error).message);
      setHotels([]);
    } finally {
      setLoading(false);
    }
  };

  const handleReserve = async () => {
    if (!selectedHotel) return;
    if (!guestName.trim() || !documentNumber.trim()) {
      setError('Guest name and document number are required.');
      return;
    }

    setError('');
    setLoading(true);

    const request: ReservationRequest = {
      destination,
      checkIn,
      checkOut,
      hotelId: selectedHotel.hotelId,
      roomType: selectedHotel.roomType,
      guestName,
      documentType,
      documentNumber,
    };

    try {
      const reservation = await reserveHotel(request);
      setConfirmation({
        reference: reservation.reference,
        provider: reservation.provider,
        totalPrice: reservation.totalPrice,
        cancellationPolicy: reservation.cancellationPolicy,
        hotelName: reservation.hotelName,
      });
      setHotels([]);
      setSelectedHotel(null);
      setGuestName('');
      setDocumentNumber('');
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <div>
          <p className="eyebrow">Hotel Stay Aggregator</p>
          <h1>Find and reserve the best hotel offers</h1>
          <p className="subtitle">Search aggregated hotel inventory from multiple providers with document validation and confirmation.</p>
        </div>
      </header>

      <main>
        <section className="panel search-panel">
          <h2>Search</h2>
          <div className="form-grid">
            <label>
              Destination
              <select value={destination} onChange={e => setDestination(e.target.value)}>
                {destinations.map(city => (
                  <option key={city} value={city}>{city}</option>
                ))}
              </select>
            </label>
            <label>
              Check-in
              <input type="date" value={checkIn} onChange={e => setCheckIn(e.target.value)} />
            </label>
            <label>
              Check-out
              <input type="date" value={checkOut} onChange={e => setCheckOut(e.target.value)} />
            </label>
            <label>
              Room type
              <select value={roomType} onChange={e => setRoomType(e.target.value as RoomType | '')}>
                <option value="">Any</option>
                {roomTypes.map(type => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
            </label>
          </div>
          <div className="action-row">
            <button type="button" onClick={handleSearch} disabled={!canSearch || loading}>
              {loading ? 'Searching…' : 'Search hotels'}
            </button>
          </div>
        </section>

        {error && <div className="notice notice-error">{error}</div>}

        {confirmation && (
          <section className="panel confirmation-panel">
            <h2>Reservation confirmed</h2>
            <p>Your booking is confirmed with reference <strong>{confirmation.reference}</strong>.</p>
            <div className="summary-grid">
              <div>
                <strong>Hotel</strong>
                <p>{confirmation.hotelName}</p>
              </div>
              <div>
                <strong>Provider</strong>
                <p>{confirmation.provider}</p>
              </div>
              <div>
                <strong>Total</strong>
                <p>${confirmation.totalPrice.toFixed(2)}</p>
              </div>
              <div>
                <strong>Cancellation</strong>
                <p>{confirmation.cancellationPolicy}</p>
              </div>
            </div>
          </section>
        )}

        <section className="panel results-panel">
          <div className="results-header">
            <h2>Results</h2>
            <p>{hotels.length} offer{hotels.length === 1 ? '' : 's'} available</p>
          </div>
          {!hotels.length && !loading && <p className="empty-state">Search to see available hotel offers.</p>}
          <div className="hotel-list">
            {sortedHotels.map(hotel => (
              <article key={hotel.hotelId} className={`hotel-card ${selectedHotel?.hotelId === hotel.hotelId ? 'selected' : ''}`} onClick={() => setSelectedHotel(hotel)}>
                <div className="hotel-tag"><span>{hotel.provider}</span></div>
                <div className="hotel-info">
                  <h3>{hotel.name}</h3>
                  <p>{hotel.roomType} • ${hotel.ratePerNight.toFixed(2)} / night</p>
                  <p className="hotel-meta">Total ${hotel.totalPrice.toFixed(2)} • {hotel.cancellationPolicy}</p>
                </div>
                <div className="hotel-badge">
                  <span>${hotel.totalPrice.toFixed(0)}</span>
                </div>
              </article>
            ))}
          </div>
        </section>

        {selectedHotel && (
          <section className="panel reservation-panel">
            <h2>Complete reservation</h2>
            <div className="selected-summary">
              <p><strong>{selectedHotel.name}</strong></p>
              <p>{selectedHotel.provider} • {selectedHotel.roomType}</p>
              <p>Total: ${selectedHotel.totalPrice.toFixed(2)}</p>
              <p>Cancellation: {selectedHotel.cancellationPolicy}</p>
            </div>
            <div className="form-grid">
              <label>
                Guest name
                <input type="text" value={guestName} onChange={e => setGuestName(e.target.value)} placeholder="Jane Doe" />
              </label>
              <label>
                Document type
                <select value={documentType} onChange={e => setDocumentType(e.target.value as DocumentType)}>
                  {documentTypes.map(type => <option key={type} value={type}>{type}</option>)}
                </select>
              </label>
              <label>
                Document number
                <input type="text" value={documentNumber} onChange={e => setDocumentNumber(e.target.value)} placeholder="Document number" />
              </label>
            </div>
            <div className="action-row">
              <button type="button" onClick={handleReserve} disabled={loading}>
                {loading ? 'Reserving…' : 'Reserve selected hotel'}
              </button>
            </div>
          </section>
        )}
      </main>
    </div>
  );
}

export default App;
