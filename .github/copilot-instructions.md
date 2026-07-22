# Repo Instructions for GitHub Copilot

## Backend
- Use Minimal API in .NET 8
- Attribute routing for clarity
- Dependency Injection for `IHotelProvider`
- Endpoints must match spec exactly:
  - `GET /hotels/search`
  - `POST /hotels/reserve`
  - `GET /hotels/reservation/{reference}`
- Include Swagger/OpenAPI via Swashbuckle.AspNetCore
- Enable CORS:
  - Allow requests from React frontend port (http://localhost:3000)
  - Backend may run on a different port (http://localhost:5042)
  - Configure in `Program.cs` using `builder.Services.AddCors` and `app.UseCors`
- Maintain a **simple layered design**:
  - Controllers → define endpoints
  - Models → represent data contracts
  - Services → encapsulate business logic and provider stubs
- This layered design is optional but strongly recommended for readability and extensibility.

## Frontend
- React JS + TypeScript
- Clean, neat, professional UI pages
- Functional components with hooks
- Centralised API calls in `services/api.ts`
- Room type selector must be a dropdown (enum values only)
- Show loading spinner during API calls
- Handle error states (empty results, validation errors)

## Tests
- NUnit framework
- Arrange‑Act‑Assert pattern
- Cover normalisation, filtering, validation, error handling
- Aim for ≥80% code coverage

## AI Usage
- Capture actual prompts and decisions in `prompts.md`
- Critically reflect on AI usage in `reflection.md`
