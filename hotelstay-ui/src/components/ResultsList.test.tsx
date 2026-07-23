import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ResultsList } from './ResultsList';

const hotels = [
  {
    provider: 'PremierStays',
    hotelId: 'PREM-101',
    name: 'Premier Ocean View',
    roomType: 'Suite' as const,
    ratePerNight: 329,
    totalPrice: 1316,
    cancellationPolicy: 'FreeCancellation' as const,
    starRating: 5,
  },
  {
    provider: 'BudgetNests',
    hotelId: 'BUD-201',
    name: 'Budget Nest Downtown',
    roomType: 'Standard' as const,
    ratePerNight: 89,
    totalPrice: 267,
    cancellationPolicy: 'Flexible' as const,
  },
];

describe('ResultsList', () => {
  it('renders available offers and sort control', () => {
    const handleSelect = vi.fn();
    const handleSortChange = vi.fn();

    render(
      <ResultsList
        hotels={hotels}
        sortDirection="asc"
        onSortDirectionChange={handleSortChange}
        onSelect={handleSelect}
      />
    );

    expect(screen.getByText('Results')).toBeInTheDocument();
    expect(screen.getByText('2 offers available')).toBeInTheDocument();
    expect(screen.getByRole('combobox')).toHaveValue('asc');
    expect(screen.getByText('Premier Ocean View')).toBeInTheDocument();
    expect(screen.getByText('Budget Nest Downtown')).toBeInTheDocument();
  });

  it('calls onSortDirectionChange when user changes sort order', async () => {
    const user = userEvent.setup();
    const handleSelect = vi.fn();
    const handleSortChange = vi.fn();

    render(
      <ResultsList
        hotels={hotels}
        sortDirection="asc"
        onSortDirectionChange={handleSortChange}
        onSelect={handleSelect}
      />
    );

    await user.selectOptions(screen.getByRole('combobox'), 'desc');
    expect(handleSortChange).toHaveBeenCalledWith('desc');
  });
});
