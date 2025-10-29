# AutoSearch.nz
a "car-listings" search aggregator.

## Design & Implementation
- Backend: .NET 9 Web API
- Frontend: React
- Database: SQLite

## Installation
```bash
git clone https://github.com/lambdv/autosearch
cd autosearch/autosearch.Server
dotnet build
dotnet run # or dotnet watch run
```
## Migrations
```bash
dotnet ef migrations add <migrationname>
dotnet ef database update
```

