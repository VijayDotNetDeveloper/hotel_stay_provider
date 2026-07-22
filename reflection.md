# Reflection

- The backend is built with .NET minimal APIs and uses in-memory dummy data to simulate provider integration.
- The frontend is implemented with React + TypeScript and focuses on a clean search/reservation experience.
- I used a layered design: models, services, and endpoint routing.
- API validation covers missing fields, invalid dates, and document mismatches with appropriate HTTP status codes.
- Challenges included ensuring the Vite scaffold completed through PowerShell restrictions; I used `cmd /c` for npm operations.
