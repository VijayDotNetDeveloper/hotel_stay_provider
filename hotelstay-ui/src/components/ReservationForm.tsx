import type { DocumentType, Hotel } from '../types';

interface ReservationFormProps {
  selectedHotel: Hotel;
  guestName: string;
  documentType: DocumentType;
  documentNumber: string;
  documentHint: string;
  onGuestNameChange: (value: string) => void;
  onDocumentTypeChange: (value: DocumentType) => void;
  onDocumentNumberChange: (value: string) => void;
  onSubmit: () => void;
  onBack: () => void;
  loading: boolean;
  error?: string;
}

const documentTypes: DocumentType[] = ['Passport', 'NationalId'];

export function ReservationForm({
  selectedHotel,
  guestName,
  documentType,
  documentNumber,
  documentHint,
  onGuestNameChange,
  onDocumentTypeChange,
  onDocumentNumberChange,
  onSubmit,
  onBack,
  loading,
  error,
}: ReservationFormProps) {
  return (
    <section className="panel reservation-panel">
      <div className="page-header-row">
        <h2>Complete reservation</h2>
        <button type="button" className="link-button" onClick={onBack}>
          Back to search
        </button>
      </div>
      <div className="selected-summary">
        <p><strong>{selectedHotel.name}</strong></p>
        <p>{selectedHotel.provider} • {selectedHotel.roomType}</p>
        <p>Total: ${selectedHotel.totalPrice.toFixed(2)}</p>
        <p>Cancellation: {selectedHotel.cancellationPolicy}</p>
      </div>
      <div className="form-grid">
        <label>
          <span className="label-title">Guest name <span className="required">*</span></span>
          <input type="text" value={guestName} onChange={e => onGuestNameChange(e.target.value)} placeholder="Jane Doe" />
        </label>
        <label>
          <span className="label-title">Document type <span className="required">*</span></span>
          <select value={documentType} onChange={e => onDocumentTypeChange(e.target.value as DocumentType)}>
            {documentTypes.map(type => <option key={type} value={type}>{type}</option>)}
          </select>
        </label>
        <label>
          <span className="label-title">Document number <span className="required">*</span></span>
          <input type="text" value={documentNumber} onChange={e => onDocumentNumberChange(e.target.value)} placeholder="Document number" />
        </label>
      </div>
      <p className="document-hint">{documentHint}</p>
      {error && <div className="notice notice-error">{error}</div>}
      <div className="action-row">
        <button type="button" onClick={onSubmit} disabled={loading}>
          {loading ? 'Reserving…' : 'Confirm reservation'}
        </button>
      </div>
    </section>
  );
}
