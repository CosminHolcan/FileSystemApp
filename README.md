# FileSystemApp

FileSystemApp is a native cloud web application that lets users securely store and manage files in Azure Blob Storage. It demonstrates a full-stack solution: a .NET 10 backend (Web API + Entity Framework Core), a React + TypeScript frontend, and Azure Functions for background maintenance tasks. The solution supports configurable redundancy, optional file versioning, and a custom cross-region redundancy mode.

## Short project summary

- Purpose: allow users to register/login and store files with metadata (region, redundancy, versioning). Users can preview, download, update files, and — when versioning is enabled — add and compare file versions.
- Technologies: .NET 10, EF Core, Azure Blob Storage, Azure Functions (isolated worker), React + TypeScript.

## Key features

- Multi-region redundancy: choose from LRS, ZRS, GRS, or a custom mode where the user selects two regions for primary + replica storage.
- Optional file versioning: when enabled, file uploads create immutable versions recorded in the database, and users can compare/download specific versions.
- File operations: upload, download, preview, rename, delete, and add versions.
- Background synchronization for custom redundancy: a dedicated Azure Function periodically inspects files that have replicas and synchronizes content and versions so both copies remain consistent.
- Storage access tier automation: weekly access-tier sweeper moves less-used blobs to Cool / Archive tiers to optimize cost.

## Architecture overview

- Backend (.NET 10)
  - Organized as a typical Web API with controllers under `server/server/Controllers`.
  - Business Logic Layer (BLL) implements higher-level operations (upload, sync, version handling) and orchestrates storage interactions.
  - Data Access Layer (DAL) uses EF Core (DbContext: `FileSystemAppDbContext`) to persist Users, AppFiles, FileVersions, and StorageAccount configuration.
  - Blob operations are performed using the Azure.Storage.Blobs SDK with `DefaultAzureCredential` (Managed Identity is used in production).

- Frontend (React + TypeScript)
  - Provides the user interface for registration/login and file management: upload, preview, download, rename, version listing and comparison.
  - The client requests SAS tokens from the backend for secure, time-limited blob access.

- Azure Functions
  - RestoreFileCopies (runs every minute): reads file pairs (original + replica), compares LastUpdate and versions, and synchronizes content/versions accordingly to enforce custom redundancy.
  - ChangeAccessTier (runs weekly, Sundays at 03:00): reads last-access timestamps and sets blob access tier to Hot / Cool / Archive based on recency.

## Custom redundancy synchronization mechanism

Custom redundancy allows a file to be stored in two different regions (two storage accounts). To keep the two copies consistent the system:

1. Persists a `ReplicaId` relationship in the database linking the original and its replica.
2. A timer-driven function (RestoreFileCopies) runs regularly and:
   - Loads all file pairs that have `ReplicaId` set.
   - Compares `LastUpdate` timestamps to find which copy is newer.
   - If neither file uses versioning, it downloads the newer blob and uploads it to the older storage (overwriting the current blob), and updates DB timestamps.
   - If versioning is enabled (either side), the function performs version-aware sync:
	 - Copies any missing version snapshots from the newer storage to the older storage and creates corresponding DB records for those versions.
	 - Ensures the latest version is available and, if necessary for client compatibility, updates the older storage current blob to reflect that latest version.
   - The BLL contains RestorePair logic; the Function invokes only BLL methods (no direct DAL calls) to remain consistent with application boundaries.

Notes on version handling
- Versions are immutable snapshots stored in blob versions + DB entries (FileVersion records with CreationTime and Azure version id).
- Clients can request SAS tokens for specific versions. If you move to always serving versioned tokens for the "current" file, you can avoid copying the latest version back to the current blob — the system can then only ensure version lists are identical between accounts.

## Security and identity

- In Azure, both the Web App and Function App use Managed Identities. They are granted RBAC roles (e.g., Blob Storage Contributor) on the storage containers they need to access.
- GitHub Actions are integrated using OIDC (OpenID Connect) so the CI/CD workflow can obtain short-lived tokens to deploy to Azure without long-lived secrets.

## GitHub workflows and branching

- There are separate cases for the `main` (web app) and `functions` branches. The repository contains dedicated workflow YAML(s) that:
  - Use OIDC for authentication to Azure.
  - Deploy the web app when changes are pushed to `main`.
  - Deploy the Function App when changes are pushed to the `functions` branch (the repository contains a YAML file that triggers on this branch).

This separation allows function implementation and deployment to be developed/tested independently from the main web app.

## Development and running locally

1. Prerequisites
   - .NET 10 SDK
   - Node.js + npm/yarn for the client
   - Azure Storage Emulator or an Azure Storage account for local testing (or Azurite)
   - An SQL Server instance for EF Core migrations (connection string in configuration)

2. Backend
   - Open `server` in Visual Studio 2022/2026 or use `dotnet` CLI.
   - Set the `DbConnection` connection string in appsettings/local settings.
   - Run EF Core migrations (they will be applied automatically on startup in Program.cs).
   - Start the server project (it hosts controllers and APIs used by the client).

3. Frontend
   - Open `client` and run `npm install` then `npm start`.
   - The UI runs on localhost and uses the backend for authentication, API calls and to request SAS tokens.

4. Functions (local run)
   - Use the Functions tools to run the Functions project locally (ensure identity/credentials for storage are available or use connection strings in local.settings.json).
