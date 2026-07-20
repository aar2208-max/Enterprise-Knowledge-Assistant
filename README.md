# Ai Doc Reader

Ai Doc Reader is a full-stack document ingestion and Q&A application. It lets users sign in, create workspaces, upload documents, and ask AI-powered questions about the documents stored in each workspace.

## How it works

- The frontend is a React + TypeScript app that handles authentication, workspace management, file upload, and chat interactions.
- The backend is an ASP.NET Core Web API that stores user workspaces and uploaded document metadata.
- Uploaded documents are saved on disk and their text is extracted for later question answering.
- A chat endpoint uses the extracted content to generate answers based on the selected workspace.

## What to prepare

Required software:
- .NET 10 SDK
- Node.js 20+ (or compatible version used by the frontend)
- A terminal / command prompt

Optional preparations:
- If you want to add a database, configure it in the backend project. The app can run with the default local configuration.
- Ensure the `frontend` and `backend` folders are available in the repository.

## Run locally

### Backend

From the repository root:

```bash
cd backend/Enterprise.Api
dotnet run
```

This starts the API and usually exposes endpoints locally.

### Frontend

From the repository root:

```bash
cd frontend
npm install
npm run dev
```

Open the URL shown by Vite (typically `http://localhost:5173`).

## App flow

1. Register or log in.
2. Create a workspace for a project or document set.
3. Upload PDF, DOCX, or TXT documents into the active workspace.
4. Select the workspace to view uploaded files.
5. Ask questions and receive answers based on the workspace content.
6. Delete files or workspaces when they are no longer needed.

## API endpoints

- `POST /api/auth/register` — create a new user account
- `POST /api/auth/login` — sign in and receive a JWT token
- `GET /api/workspaces` — list workspaces for the current user
- `POST /api/workspaces` — create a new workspace
- `DELETE /api/workspaces/{workspaceId}` — delete a workspace
- `GET /api/workspaces/{workspaceId}/documents` — list uploaded documents for a workspace
- `DELETE /api/workspaces/{workspaceId}/documents/{documentId}` — delete a document
- `POST /api/documents/upload` — upload a document file
- `POST /api/chat/ask` — ask a question against workspace documents

## Notes

- Uploaded documents are stored on disk under the backend project so make sure the backend process has write permission.
- The app uses JWT authentication, so the frontend stores the token locally during a session.
- The current implementation is designed as a developer preview and may be extended with production-ready persistence and stronger validation.
