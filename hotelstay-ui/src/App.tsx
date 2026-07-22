import { useMemo, useState } from 'react';
import './App.css';
import type { Hotel, ReservationRequest, RoomType, DocumentType } from './types';
import { searchHotels, reserveHotel } from './services/api';
import { SearchForm } from './components/SearchForm';
import { ResultsList } from './components/ResultsList';
import { ReservationForm } from './components/ReservationForm';
import { ConfirmationView } from './components/ConfirmationView';

type PageView = 'search' | 'reservation' | 'confirmation';

const internationalDestinations = ['Paris', 'Tokyo', 'London'];

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
  const [searchError, setSearchError] = useState('');
  const [reservationError, setReservationError] = useState('');
  const [page, setPage] = useState<PageView>('search');
  const [confirmation, setConfirmation] = useState<null | {
    reference: string;
    provider: string;
    totalPrice: number;
    cancellationPolicy: string;
    hotelName: string;
  }>(null);

  const sortedHotels = useMemo(() => [...hotels].sort((a, b) => a.totalPrice - b.totalPrice), [hotels]);
  const canSearch = Boolean(destination && checkIn && checkOut);
  const isInternationalDestination = internationalDestinations.includes(destination);
  const documentHint = isInternationalDestination
    ? 'Passport is required for international destinations.'
    : 'National ID is required for domestic destinations.';

  const handleSearch = async () => {
    setSearchError('');
    setReservationError('');
    setConfirmation(null);
    setSelectedHotel(null);
    setPage('search');

    if (!canSearch) {
      setSearchError('Please complete destination, check-in, and check-out before searching.');
      return;
    }

    if (checkOut <= checkIn) {
      setSearchError('Check-out must be later than check-in.');
      return;
    }

    setLoading(true);
    try {
      const results = await searchHotels(destination, checkIn, checkOut, roomType || undefined);
      setHotels(results);
      if (!results.length) {
        setSearchError('No available hotel offers found for the selected criteria.');
      }
    } catch (err) {
      setSearchError((err as Error).message);
      setHotels([]);
    } finally {
      setLoading(false);
    }
  };

  const startReservation = (hotel: Hotel) => {
    setSelectedHotel(hotel);
    setReservationError('');
    setSearchError('');
    setPage('reservation');
  };

  const handleReserve = async () => {
    if (!selectedHotel) return;
    if (!guestName.trim() || !documentNumber.trim()) {
      setReservationError('Guest name and document number are required.');
      return;
    }

    setReservationError('');
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
      setPage('confirmation');
      setHotels([]);
      setSelectedHotel(null);
      setGuestName('');
      setDocumentNumber('');
    } catch (err) {
      setReservationError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const resetToSearch = () => {
    setPage('search');
    setConfirmation(null);
    setReservationError('');
    setSearchError('');
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
        {page === 'search' && (
          <>
            <SearchForm
              destination={destination}
              checkIn={checkIn}
              checkOut={checkOut}
              roomType={roomType}
              onDestinationChange={setDestination}
              onCheckInChange={setCheckIn}
              onCheckOutChange={setCheckOut}
              onRoomTypeChange={setRoomType}
              onSubmit={handleSearch}
              loading={loading}
              canSearch={canSearch}
            />
            {searchError && <div className="notice notice-error">{searchError}</div>}
            <ResultsList hotels={sortedHotels} onSelect={startReservation} />
          </>
        )}

        {page === 'reservation' && selectedHotel && (
          <ReservationForm
            selectedHotel={selectedHotel}
            guestName={guestName}
            documentType={documentType}
            documentNumber={documentNumber}
            onGuestNameChange={setGuestName}
            onDocumentTypeChange={setDocumentType}
            onDocumentNumberChange={setDocumentNumber}
            onSubmit={handleReserve}
            onBack={resetToSearch}
            loading={loading}
            error={reservationError}
            documentHint={documentHint}
          />
        )}

        {page === 'confirmation' && confirmation && (
          <ConfirmationView
            reference={confirmation.reference}
            provider={confirmation.provider}
            totalPrice={confirmation.totalPrice}
            cancellationPolicy={confirmation.cancellationPolicy}
            hotelName={confirmation.hotelName}
            onReturn={resetToSearch}
          />
        )}
      </main>
    </div>
  );
}

export default App;
