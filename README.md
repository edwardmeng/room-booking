# Room Booking API

This repository contains the implemented .NET 10 room-booking API. It uses ASP.NET Core controllers, EF Core, and file-backed SQLite with strict request validation and RFC 7807 error responses.

## Projects

- `RoomBooking.Api`: API Controllers, HTTP Client Models, OpenAPI, Problem Details, and dependency composition.
- `RoomBooking.BusinessContracts`: service interfaces, result types, and Domain Models in one namespace.
- `RoomBooking.BusinessServices`: room and reservation business services.
- `RoomBooking.Common`: shared functions.
- `RoomBooking.DataAccess`: EF Core entities, mappings, DbContext, and SQLite registration.
- `RoomBooking.UnitTests`: executable business-rule and service tests.
- `RoomBooking.IntegrationTests`: executable HTTP API tests; test cases interact with the application through `HttpClient`.

## Commands

```powershell
dotnet restore RoomBooking.slnx
dotnet build RoomBooking.slnx --configuration Release
dotnet test RoomBooking.slnx --configuration Release --no-build
dotnet run --project src/RoomBooking.Api
```

## API documentation

Running the API with the default `http` launch profile enables interactive API documentation in the Development environment:

- Scalar API reference: <http://localhost:5028/scalar/v1>
- OpenAPI document: <http://localhost:5028/openapi/v1.json>

The Scalar UI can be used to inspect and execute the room and reservation endpoints. Both documentation endpoints are disabled outside the Development environment.