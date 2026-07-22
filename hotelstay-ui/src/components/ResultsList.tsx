import type { Hotel } from '../types';

interface ResultsListProps {
  hotels: Hotel[];
  onSelect: (hotel: Hotel) => void;
}

export function ResultsList({ hotels, onSelect }: ResultsListProps) {
  return (
    <section className="panel results-panel">
      <div className="results-header">
        <h2>Results</h2>
        <p>{hotels.length} offer{hotels.length === 1 ? '' : 's'} available</p>
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
