interface ConfirmationViewProps {
  reference: string;
  provider: string;
  totalPrice: number;
  cancellationPolicy: string;
  hotelName: string;
  onReturn: () => void;
}

export function ConfirmationView({ reference, provider, totalPrice, cancellationPolicy, hotelName, onReturn }: ConfirmationViewProps) {
  return (
    <section className="panel confirmation-panel">
      <h2>Reservation confirmed</h2>
      <p>Your booking is confirmed with reference <strong>{reference}</strong>.</p>
      <div className="summary-grid">
        <div>
          <strong>Hotel</strong>
          <p>{hotelName}</p>
        </div>
        <div>
          <strong>Provider</strong>
          <p>{provider}</p>
        </div>
        <div>
          <strong>Total</strong>
          <p>${totalPrice.toFixed(2)}</p>
        </div>
        <div>
          <strong>Cancellation</strong>
          <p>{cancellationPolicy}</p>
        </div>
      </div>
      <div className="action-row">
        <button type="button" onClick={onReturn}>
          Search again
        </button>
      </div>
    </section>
  );
}
