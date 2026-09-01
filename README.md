# Smart Real Estate — ASP.NET Core + SQL Server

Professional .NET 10 REST API implementing the Smart Real Estate feature set with Clean Architecture, EF Core, SQL Server, JWT authentication, role-based authorization, bookings, favorites, reviews, property moderation, image storage, Swagger, and Docker.

## Architecture

```text
RealEstate/
├── src/
│   ├── RealEstate.Api/
│   │   ├── Controllers/
│   │   ├── Extensions/
│   │   ├── Middleware/
│   │   └── Models/
│   ├── RealEstate.Application/
│   │   ├── Common/
│   │   ├── Features/
│   │   └── Interfaces/
│   ├── RealEstate.Domain/
│   │   ├── Entities/
│   │   └── Enums/
│   └── RealEstate.Infrastructure/
│       ├── Persistence/
│       │   └── Configurations/
│       └── Services/
├── tests/
├── docs/
├── docker-compose.yml
└── RealEstate.sln
```

## Features

- JWT authentication with User / Vendor / Admin roles
- Property CRUD and admin approval/rejection workflow
- Search, filtering, sorting and pagination
- Sale / Rent listings
- Image upload with validation and cleanup
- Booking flow: Pending / Confirmed / Rejected / Cancelled
- Favorites / Wishlist
- Reviews and ratings with one review per user/property
- Admin dashboard, users and property moderation
- EF Core + SQL Server with indexes and transient-failure retry
- Centralized exception handling with trace IDs
- Swagger/OpenAPI with Bearer authentication
- Docker Compose for API + SQL Server

## Local setup

Install the .NET 10 SDK and SQL Server, then configure `src/RealEstate.Api/appsettings.json` or environment variables.

```bash
dotnet restore RealEstate.sln
dotnet build RealEstate.sln
dotnet run --project src/RealEstate.Api/RealEstate.Api.csproj
```

Swagger: `/swagger`

For a fresh development database, the initializer can create the schema when the database is not reachable. For real deployments, create and apply EF Core migrations:

```bash
dotnet ef migrations add InitialCreate --project src/RealEstate.Infrastructure/RealEstate.Infrastructure.csproj --startup-project src/RealEstate.Api/RealEstate.Api.csproj
dotnet ef database update --project src/RealEstate.Infrastructure/RealEstate.Infrastructure.csproj --startup-project src/RealEstate.Api/RealEstate.Api.csproj
```

## Docker

```bash
docker compose up --build
```

API: `http://localhost:8080`
SQL Server: `localhost,1433`

## Seed accounts

- Admin: `admin@smartrealestate.local` / `Password123!`
- Vendor: `vendor@smartrealestate.local` / `Password123!`

Change these credentials before any non-local deployment.

## Main endpoints

```text
POST   /api/auth/register
POST   /api/auth/login

GET    /api/properties
GET    /api/properties/search
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
```
