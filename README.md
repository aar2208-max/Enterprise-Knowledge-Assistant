# Enterprise Knowledge Assistant

A full-stack RAG application for enterprise document search and Q&A.

## Backend

The backend is now implemented as a working ASP.NET Core API with:
- authentication endpoints for registration and login
- workspace creation
- document upload metadata handling
- chat/question answering over uploaded workspace documents
- Swagger documentation

## Run locally

### Backend

```bash
dotnet run --project backend/Enterprise.Api/Enterprise.Api.csproj
```

The API will be available at:
- http://localhost:5000
- http://localhost:5001

### API endpoints

- POST /api/auth/register
- POST /api/auth/login
- POST /api/workspaces
- POST /api/documents/upload
- POST /api/chat/ask
- GET /api/health

## Architecture

- Angular frontend
- .NET API
- Python AI service
- PostgreSQL / In-memory fallback for local development
- OpenAI-compatible AI integration point
