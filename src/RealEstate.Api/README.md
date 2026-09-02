# API layer

`RealEstate.Api` is the HTTP boundary of the application.

It contains controllers, HTTP-specific request models, middleware, Swagger/OpenAPI setup, authentication/authorization wiring, and dependency composition.

The API delegates use cases to Application abstractions. Domain entities own domain rules, while Infrastructure owns EF Core/SQL Server persistence, file storage, and other technical implementations.

Controllers should stay thin: bind HTTP input, obtain the current user context when needed, invoke an application abstraction, and translate the result into an HTTP response.
