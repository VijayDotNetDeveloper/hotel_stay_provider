import type { RoomType } from '../types';

interface SearchFormProps {
  destination: string;
  checkIn: string;
  checkOut: string;
  roomType: RoomType | '';
  onDestinationChange: (value: string) => void;
  onCheckInChange: (value: string) => void;
  onCheckOutChange: (value: string) => void;
  onRoomTypeChange: (value: RoomType | '') => void;
  onSubmit: () => void;
  loading: boolean;
  canSearch: boolean;
}

const destinations = ['Seattle', 'Portland', 'New York', 'Paris', 'Tokyo', 'London'];
const roomTypes: RoomType[] = ['Standard', 'Deluxe', 'Suite'];

export function SearchForm({
  destination,
  checkIn,
  checkOut,
  roomType,
  onDestinationChange,
  onCheckInChange,
  onCheckOutChange,
  onRoomTypeChange,
  onSubmit,
  loading,
  canSearch,
}: SearchFormProps) {
  return (
    <section className="panel search-panel">
      <h2>Search</h2>
      <div className="form-grid">
        <label>
          <span className="label-title">Destination <span className="required">*</span></span>
          <select value={destination} onChange={e => onDestinationChange(e.target.value)}>
            {destinations.map(city => (
              <option key={city} value={city}>{city}</option>
            ))}
          </select>
        </label>
        <label>
          <span className="label-title">Check-in <span className="required">*</span></span>
          <input type="date" value={checkIn} onChange={e => onCheckInChange(e.target.value)} />
        </label>
        <label>
          <span className="label-title">Check-out <span className="required">*</span></span>
          <input type="date" value={checkOut} onChange={e => onCheckOutChange(e.target.value)} />
        </label>
        <label>
          <span className="label-title">Room type</span>
          <select value={roomType} onChange={e => onRoomTypeChange(e.target.value as RoomType | '')}>
            <option value="">Any</option>
            {roomTypes.map(type => (
              <option key={type} value={type}>{type}</option>
            ))}
          </select>
        </label>
      </div>
      <div className="action-row">
        <button type="button" onClick={onSubmit} disabled={!canSearch || loading}>
          {loading ? 'Searching…' : 'Search hotels'}
        </button>
      </div>
    </section>
  );
}
