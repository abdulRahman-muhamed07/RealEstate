# Smart Real Estate — ASP.NET Core + SQL Server

Professional .NET backend implementation of the Smart Real Estate platform, following Clean Architecture and the same feature set as the reference smart-real-estate project.

## Architecture

```text
src/
├── RealEstate.Api            # HTTP layer, controllers, JWT, Swagger
├── RealEstate.Application    # DTOs, contracts, use-case abstractions
├── RealEstate.Domain         # Entities and domain enums
└── RealEstate.Infrastructure # EF Core, SQL Server, authentication, file storage
```

## Features

- JWT authentication with User / Vendor / Admin roles
- Property CRUD with approval workflow
- Search, filtering, sorting and pagination
- Sale / Rent listings
- Property image upload (up to 8 images, 5 MB each)
- Booking flow: Pending / Confirmed / Rejected / Cancelled
- Favorites / Wishlist
- Reviews and ratings with one review per user/property
- Admin dashboard, users and property moderation
- EF Core + SQL Server with indexes and retry-on-failure
- Swagger/OpenAPI with Bearer authentication
- Docker + SQL Server Compose setup

## Local setup

1. Install .NET 10 SDK and SQL Server.
2. Update `src/RealEstate.Api/appsettings.json` with your SQL Server connection string and a strong JWT key.
3. Run:

```bash
dotnet restore RealEstate.sln
dotnet build RealEstate.sln
dotnet run --project src/RealEstate.Api/RealEstate.Api.csproj
```

The API is exposed by the launch profile on the configured localhost port and Swagger is available at `/swagger`.

The application creates the database schema on first run for development. For production, use EF Core migrations and a managed SQL Server instance.

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
