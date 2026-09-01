# Smart Real Estate — ASP.NET Core + SQL Server

Backend for a real-estate platform that supports property listings, search, bookings, favorites, reviews, and admin moderation. The API is implemented with ASP.NET Core, EF Core, and SQL Server.

## Project structure

```text
RealEstate/
├── src/
│   ├── RealEstate.Api/            # HTTP layer, controllers and middleware
│   ├── RealEstate.Application/    # Feature contracts, use-case abstractions and results
│   ├── RealEstate.Domain/         # Entities and domain enums
│   └── RealEstate.Infrastructure/ # EF Core, SQL Server, authentication and storage
├── tests/
│   └── RealEstate.UnitTests/
├── docs/
├── .github/workflows/ci.yml
├── docker-compose.yml
└── RealEstate.sln
```

## Main features

- JWT authentication with `User`, `Vendor`, and `Admin` roles
- Property create/update/delete with admin approval
- Search, filtering, sorting, and page-based pagination
- Sale and rent listings
- Property images with size/type/count validation
- Booking workflow with pending, confirmed, rejected, and cancelled states
- Favorites and reviews
- Admin dashboard and moderation endpoints
- SQL Server persistence through EF Core
- Centralized exception handling with trace IDs
- Swagger/OpenAPI with Bearer authentication
- Docker Compose for local API + SQL Server development

## A few design decisions

### Why SQL Server + EF Core?

The project uses relational data with clear relationships between users, properties, bookings, favorites, and reviews. EF Core keeps the data model and queries close to the application while SQL Server handles constraints, indexes, and transactions.

### Why no generic repository?

EF Core already provides a unit-of-work and repository-like abstraction through `DbContext` and `DbSet`. A generic repository would add another abstraction without solving a current problem, so data access stays in the infrastructure services where the queries are actually needed.

### Why keep file storage behind an interface?

The application only knows about `IImageStorage`. The current implementation stores files locally for development, while a cloud-backed implementation can be added later without changing the application contracts.

### How is booking consistency handled?

The service validates the current property state and booking state before changing a booking. Database constraints are also used where uniqueness matters, so concurrent requests do not rely on application checks alone.

## Local setup

Install the .NET 10 SDK and SQL Server.

```bash
dotnet restore RealEstate.sln
dotnet build RealEstate.sln
dotnet run --project src/RealEstate.Api/RealEstate.Api.csproj
```

Swagger is available at `/swagger`.

For a new database, create and apply the EF Core migrations from the solution root:

```bash
dotnet ef migrations add InitialCreate --project src/RealEstate.Infrastructure/RealEstate.Infrastructure.csproj --startup-project src/RealEstate.Api/RealEstate.Api.csproj
dotnet ef database update --project src/RealEstate.Infrastructure/RealEstate.Infrastructure.csproj --startup-project src/RealEstate.Api/RealEstate.Api.csproj
```

Development seed data is only enabled in the Development environment.

## Docker

```bash
docker compose up --build
```

The compose setup starts SQL Server first, waits for its health check, and then starts the API.

API: `http://localhost:8080`
SQL Server: `localhost,1433`

The local development seed accounts are documented for local use only. Change them before using the project outside a development environment.

## API overview

```text
POST   /api/auth/register
POST   /api/auth/login

GET    /api/properties
GET    /api/properties/{id}
POST   /api/properties
PUT    /api/properties/{id}
DELETE /api/properties/{id}
GET    /api/properties/mine
GET    /api/properties/admin/pending
PATCH  /api/properties/admin/{id}/status

POST   /api/bookings
GET    /api/bookings/my-bookings
GET    /api/bookings/vendor/all
PATCH  /api/bookings/{id}/cancel
PATCH  /api/bookings/{id}/confirm
PATCH  /api/bookings/{id}/reject

GET    /api/favorites
POST   /api/favorites
DELETE /api/favorites/{propertyId}

GET    /api/reviews/property/{propertyId}
POST   /api/reviews
PUT    /api/reviews/{id}
DELETE /api/reviews/{id}
GET    /api/reviews/my-reviews

GET    /api/admin/dashboard
GET    /api/admin/users
DELETE /api/admin/users/{id}
GET    /api/admin/properties
DELETE /api/admin/properties/{id}
```
