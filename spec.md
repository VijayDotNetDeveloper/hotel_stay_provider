# Hotel Stay Aggregator – Specification

## Overview
A full‑stack web application built with:
- Backend: .NET 8 Minimal API (C#)
- Frontend: React JS + TypeScript
- Tests: NUnit
- Data: In‑memory dummy data (no database)

---

## Functional Requirements

### Providers
- **PremierStays**
  - PascalCase JSON
  - Always available
  - Full details: rate, cancellation policy, amenities, star rating
- **BudgetNests**
  - snake_case JSON
  - Must return deterministic `available=false` cases
  - Minimal details: rate and cancellation policy only

Both providers support `Standard`, `Deluxe`, `Suite` → unified enum.

### Document Validation
- International destinations → Passport required
- Domestic destinations → National ID accepted
- Define at least 2 domestic and 3 international cities
- Validate client‑side and server‑side
- Return `422` with clear message on mismatch

---

## API Endpoints
- `GET /hotels/search?destination={city}&checkIn={date}&checkOut={date}&roomType={type}`
  - `roomType` is optional
  - Errors:
    - `400` if required params missing
    - `400` if `checkOut <= checkIn`
- `POST /hotels/reserve`
- `GET /hotels/reservation/{reference}`

---

## Backend Notes
- Include Swagger/OpenAPI UI (`/swagger`) for documentation and testing.
- Enable CORS to allow requests from React frontend port (e.g., 3000) to backend port (e.g., 5042).
- Maintain a **simple layered design** for readability:
  - Controllers → endpoints
  - Models → data contracts
  - Services → business logic, provider stubs

---

## Frontend
- Clean, neat, professional UI with React + TypeScript
- Search form: destination, check‑in, check‑out, optional room type (dropdown selector)
- Results list: provider badge, room type, per‑night rate, total price, cancellation policy
  - Sortable by total price
- Reservation form: guest name, document type, document number
- Confirmation screen: reference number, provider, total price, cancellation policy
- States: loading spinner, results, empty, error, confirmation

---

## Models
- `Hotel`: Id, Name, Provider, RoomType, RatePerNight, TotalPrice, CancellationPolicy, Amenities, StarRating
- `Reservation`: ReferenceNumber, HotelId, GuestName, DocumentType, DocumentNumber, CheckIn, CheckOut
- Enums: RoomType, DocumentType, CancellationPolicy

---

## Tests
- NUnit tests for validation, normalisation, filtering, error handling
- Coverage Goal: ≥80% of business logic
