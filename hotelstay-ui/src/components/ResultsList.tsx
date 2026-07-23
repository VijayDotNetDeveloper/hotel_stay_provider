import type { Hotel } from '../types';

interface ResultsListProps {
  hotels: Hotel[];
  sortDirection: 'asc' | 'desc';
  onSortDirectionChange: (direction: 'asc' | 'desc') => void;
  onSelect: (hotel: Hotel) => void;
}

export function ResultsList({ hotels, sortDirection, onSortDirectionChange, onSelect }: ResultsListProps) {
  return (
    <section className="panel results-panel">
      <div className="results-header">
        <div>
          <h2>Results</h2>
          <p>{hotels.length} offer{hotels.length === 1 ? '' : 's'} available</p>
        </div>
        <label className="sort-control">
          <span>Sort by total price</span>
          <select value={sortDirection} onChange={e => onSortDirectionChange(e.target.value as 'asc' | 'desc')}>
            <option value="asc">Lowest first</option>
            <option value="desc">Highest first</option>
          </select>
        </label>
      </div>
      {!hotels.length ? (
        <p className="empty-state">Search to see available hotel offers.</p>
      ) : (
        <div className="hotel-list">
          {hotels.map(hotel => (
            <article className="hotel-card" key={hotel.hotelId}>
              <div className="hotel-tag"><span>{hotel.provider}</span></div>
              <div className="hotel-info">
                <h3>{hotel.name}</h3>
                <p>{hotel.roomType} • ${hotel.ratePerNight.toFixed(2)} / night</p>
                <p className="hotel-meta">Total ${hotel.totalPrice.toFixed(2)} • {hotel.cancellationPolicy}</p>
              </div>
              <div className="hotel-badge">
                <span>{hotel.starRating ? `${hotel.starRating} ★` : 'No rating'}</span>
              </div>
              <button type="button" className="reserve-button" onClick={() => onSelect(hotel)}>
                Reserve
              </button>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
