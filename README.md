# AgileActors.Api

## Overview
AgileActors.Api is a .NET 9 Web API that aggregates data from multiple external providers (NewsAPI, Spotify, OpenWeather).

## Features
- Aggregation of external APIs
- JWT authentication
- In-memory caching
- Resilience (retry, timeout, circuit breaker)
- Performance monitoring with Quartz
- OpenAPI/Scalar documentation

## Endpoints
- `GET /api/aggregate/public` → Aggregate data (no auth)
- `GET /api/aggregate/secure` → Aggregate data (requires JWT)
- `POST /api/auth/login` → Get JWT token
- `GET /api/stats/public` → Performance stats

## Setup
1. Clone repo
2. Configure API keys in `appsettings.json`
3. Run with `dotnet run`

## Auth
- Demo login: username = `test`, password = `123`
- Use token in Authorization header: `Bearer <token>`

## Development
- Swagger/OpenAPI available
- Scalar UI enabled in Development
