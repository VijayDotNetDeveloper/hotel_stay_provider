import { render, screen } from '@testing-library/react';
import { SearchForm } from './SearchForm';

describe('SearchForm', () => {
  it('renders form fields and enables search button when valid', () => {
    const props = {
      destination: 'Seattle',
      checkIn: '2026-08-01',
      checkOut: '2026-08-05',
      roomType: '',
      onDestinationChange: vi.fn(),
      onCheckInChange: vi.fn(),
      onCheckOutChange: vi.fn(),
      onRoomTypeChange: vi.fn(),
      onSubmit: vi.fn(),
      loading: false,
      canSearch: true,
    };

    render(<SearchForm {...props} />);

    expect(screen.getByLabelText(/Destination/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Check-in/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Check-out/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Room type/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Search hotels/i })).toBeEnabled();
  });
});
