# hotel_stay_provider

A hotel availability and reservation application with a .NET backend and React frontend.

## Overview

- Backend: .NET 9 Minimal API with Swagger/OpenAPI, CORS, versioned routes, provider aggregation, and reservation processing.
- Frontend: React + TypeScript using Vite, componentized UI, centralized API service, and environment-driven backend configuration.

## Backend

- Project: `HotelStay.Api`
- Implemented endpoints:
  - `GET /hotels/search`
  - `POST /hotels/reserve`
  - `GET /hotels/reservation/{reference}`
  - `GET /api/v1/health`
- Features:
  - `IHotelProvider` abstraction for multiple hotel providers
  - `HotelSearchService` for search aggregation and normalization
  - `ReservationService` for validation, booking, and reservation storage
  - In-memory reservation repository for demo/testing purposes
  - JSON enum serialization and Swagger UI exposed in development
  - CORS configured using `Cors:AllowedOrigins` from `HotelStay.Api/appsettings.json`

## Frontend

- Project: `hotelstay-ui`
- Uses component separation:
  - `SearchForm`
  - `ResultsList`
  - `ReservationForm`
  - `ConfirmationView`
- API configuration is environment-driven via `.env` and `import.meta.env.VITE_API_BASE`
- Default frontend backend base URL:
  - `http://localhost:5042`
- Frontend tests run with `npm test` in the `hotelstay-ui` folder.

## Setup

### Backend

```powershell
cd HotelStay.Api
dotnet restore
dotnet run
```

### Frontend

```powershell
cd hotelstay-ui
npm install
npm run dev
```

## Environment

Frontend base API URL is configured in `hotelstay-ui/.env`:

```text
VITE_API_BASE=http://localhost:5042/api/v1
```

## Testing

- Backend tests: `HotelStay.Tests`
- Frontend tests: `hotelstay-ui` using Vitest and Testing Library
- Includes unit tests, API integration tests, and React component tests
- Run tests with:

```powershell
cd HotelStay.Tests
dotnet test
```

## Notes

- The current reservation repository is in-memory and suitable for demo/testing only.
- For production, replace the repository with a persistent store and add centralized error handling.
