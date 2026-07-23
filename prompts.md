# Prompts

- Prompt: "Implement the Hotel Stay Aggregator backend using .NET Minimal API with routes `/hotels/search`, `/hotels/reserve`, and `/hotels/reservation/{reference}`. Ensure search uses `checkIn` and `checkOut` query parameters exactly."
  - Decision: Use controller-less minimal route mapping and explicit model validation to comply with spec and remove previous MVC controllers.
- Prompt: "Ensure `roomType` is optional for search and `BudgetNests` can return deterministic unavailable offers while filtering unavailable results in search aggregation."
  - Decision: Keep `RoomType?` throughout search models and service methods; implement deterministic availability calculation in `BudgetNestsProvider`; filter `Available == false` in `HotelSearchService`.
- Prompt: "Align cancellation policy values with the spec by removing `Limited` and mapping BudgetNests `limited` to `Flexible` while preserving PremierStays `FreeCancellation` and `NonRefundable`."
  - Decision: Update shared enum and backend normalization logic; update frontend type definitions and tests accordingly.
- Prompt: "Add user-facing sort control for hotel results so users can choose ascending or descending total price." 
  - Decision: Add sort direction state in `App.tsx` and expose a dropdown in `ResultsList.tsx`.
- Prompt: "Add frontend component tests for `ResultsList` and `SearchForm`, using Vitest and Testing Library, to satisfy spec coverage requirements." 
  - Decision: Configure vitest with jsdom, create component tests, and document frontend test execution in README.
- Prompt: "Capture actual AI prompt usage and decision rationale in prompts.md, and reflect on AI tooling usage and improvement areas in reflection.md."
  - Decision: Expand prompts and reflection documentation with specific prompts, responses, and learning notes.

## Implementation notes

- Converted backend to .NET Minimal API in `HotelStay.Api/Program.cs`.
- Removed controller-based routing and residual controller files.
- Added `ResultsList` sort UI and frontend test coverage.
- Updated cancellation policy enum and provider mapping to match spec.
- Separated provider seed data from reservation storage in `DataService`.
