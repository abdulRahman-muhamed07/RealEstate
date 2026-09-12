# Local setup

This file documents the local SQL Server prerequisite for the development build.

1. Make sure SQL Server or SQL Server LocalDB is installed and running.
2. The default development connection string uses `localhost` and database `RealEstateDb`.
3. If your SQL Server is a named instance, update `src/RealEstate.Api/appsettings.json` or provide `ConnectionStrings__DefaultConnection` as an environment variable.
4. Restore and build the solution before running the API.
