# WorkPac Recruitment Platform

A scalable system for managing job applications and matching candidates to jobs in the mining, industrial, and engineering sectors.

## Architecture

Microservices-based platform with event-driven communication:

- **Applications API** — Submit, withdraw, track applications; status workflow
- **Matching Service** — Background worker scoring candidates against jobs
- **Compliance API** — Verify certifications, licenses, medicals
- **Event Bus** — Async messaging (RabbitMQ local / Service Bus production)

See [architecture.md](docs/architecture.md), [data-model.md](docs/data-model.md), and [matching-algorithm.md](docs/matching-algorithm.md) for full details.

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run locally

A PowerShell run script is provided for convenience:

```powershell
.\run.ps1 -All                    # Full end-to-end with RabbitMQ (requires Docker)
.\run.ps1 -Api -Mode Local        # API only, in-memory (no Docker)
.\run.ps1 -Infrastructure         # Just start Docker containers
```

#### Default mode (InMemory — no Docker required)

```bash
dotnet run --project src/Services/WorkPac.Recruitment.Applications.Api
```

The API is available at `http://localhost:5001/swagger`. Data is seeded at startup (5 candidates, 3 jobs, 3 applications with documents). Note: InMemory event bus is per-process, so the Matching Service won't receive events when run separately — use RabbitMQ mode for cross-process matching.

#### RabbitMQ mode (end-to-end matching across services)

```powershell
.\run.ps1 -All
```

Or manually:

```bash
docker compose up -d
$env:InfrastructureMode="RabbitMQ"
dotnet run --project src/Services/WorkPac.Recruitment.Applications.Api  # terminal 1
$env:InfrastructureMode="RabbitMQ"
dotnet run --project src/Services/WorkPac.Recruitment.Matching.Service   # terminal 2
```

#### Infrastructure modes

| Mode | Repositories | Event Bus | Storage | Docker required |
|---|---|---|---|---|
| `Local` (default) | InMemory | InMemory | Local file | No |
| `RabbitMQ` | InMemory | RabbitMQ | Local file | RabbitMQ only |
| `Azure` | SQL Server | RabbitMQ | Local file | All services |

### Run tests

```bash
dotnet test
```

## Project Structure

```
src/
├── Shared/                    # Domain primitives, value objects, enums
│   ├── WorkPac.Recruitment.Shared/
│   └── WorkPac.Recruitment.Contracts/    # DTOs, message contracts
├── Infrastructure/            # EF Core, messaging, storage, middleware
│   └── WorkPac.Recruitment.Infrastructure/
└── Services/
    ├── WorkPac.Recruitment.Applications.Api/     # Primary API
    ├── WorkPac.Recruitment.Matching.Service/      # Scoring engine
    └── WorkPac.Recruitment.Compliance.Api/        # Compliance stub
tests/
├── Matching.Service.Tests/    # 46 tests — unit + orchestration (scorers, engine, service)
├── Applications.Api.Tests/    # 18 integration tests (incl. end-to-end matching flow)
└── Architecture.Tests/        # 5 architectural constraint tests
infrastructure/
└── terraform/                 # Azure IaC (illustrative)
.github/workflows/             # CI/CD pipelines
docs/
├── architecture.md            # System design document
├── data-model.md              # ERD and entity descriptions
├── matching-algorithm.md      # Scoring formula and weights
└── diagrams/                  # PlantUML diagrams
```

## API Endpoints

| Method | Path | Description |
|---|---|---|
| POST | `/v1/jobs/{jobId}/applications` | Submit application |
| GET | `/v1/jobs/{jobId}/applications` | List applications for job (paginated) |
| GET | `/v1/applications/{id}` | Get application detail |
| PATCH | `/v1/applications/{id}/status` | Update application status |
| POST | `/v1/applications/{id}/documents` | Upload document |
| DELETE | `/v1/applications/{id}` | Withdraw application |
| GET | `/v1/candidates/{id}/applications` | List applications by candidate (paginated) |
| GET | `/v1/documents` | List all documents across applications (paginated) |
| GET | `/v1/applications/{id}/documents` | List documents for an application (paginated) |
| GET | `/v1/jobs` | List jobs (paginated, page & pageSize params) |
| GET | `/v1/candidates` | List candidates (paginated, page & pageSize params) |
| GET | `/health` | Health check |

Query parameters `page` (default 1) and `pageSize` (default 20) are supported on all list endpoints.

### Compliance API

The Compliance API (`src/Services/WorkPac.Recruitment.Compliance.Api/`) is currently a **stub** that demonstrates the endpoint contract with hardcoded responses:

| Method | Path | Description |
|---|---|---|
| POST | `/v1/applications/{id}/compliance-checks` | Initiate compliance checks |
| GET | `/v1/applications/{id}/compliance-checks` | Get compliance check status |

- **`POST`** — Accepts an `InitiateComplianceCheckRequest` (list of check types), validates it with FluentValidation, and returns all checks (Medical, Induction, License) as `"Pending"`.
- **`GET`** — Returns hardcoded results: Medical and License as `"Pass"`, Induction as `"Pending"`.

In production, this service would persist compliance results per application, integrate with external verification providers (medical clinics, licensing authorities), and use async event-driven updates via the message bus.

## Authentication & Authorization

For production, authentication is handled by **Azure AD B2C** with an Azure API Management (APIM) gateway:

- **Recruiters/Admins** authenticate via Azure AD B2C integrated with the corporate IdP (Azure AD)
- **Candidates** authenticate via Azure AD B2C with social identity providers (Microsoft, Google, LinkedIn)
- **API tokens** issued after authentication, validated by APIM before reaching backend services
- **Role-based authorization** enforced at the service level via `[Authorize]` attributes:
  - `Recruiter` — full access to all endpoints
  - `Candidate` — limited to own applications and profile
  - `Admin` — elevated access including compliance verification

The local development profile runs with `anonymousAuthentication: true`. To enable auth locally, set up a local IdentityServer or Azure AD B2C test tenant and configure the OpenID Connect middleware.

### Planned Implementation

```
Azure AD B2C → APIM (validate tokens) → Applications API ([Authorize]) + Matching Service (validated via APIM)
```

## API Versioning

The API uses **URL path versioning** (`/v1/`, `/v2/`). The current version is `v1` and is expected to remain stable.

### Versioning Strategy

| Component | Approach |
|---|---|
| **Path prefix** | `/v{major}/` in route templates (e.g., `/v1/jobs`) |
| **Futures** | Support content-type versioning (`application/vnd.workpac.v2+json`) and query-string fallback (`?api-version=2.0`) via APIM |
| **Deprecation** | Deprecated versions serve `Sunset` and `Deprecation` HTTP headers; Swagger docs mark them accordingly |
| **Backward compatibility** | New fields added as optional; breaking changes require a new major version |

Local dev uses hardcoded `/v1` prefixes. Production would add `Microsoft.AspNetCore.Mvc.Versioning` for attribute-based versioning and API explorer integration.

## Candidate Matching Score

`Score = 0.30×Skills + 0.25×Experience + 0.15×Location + 0.20×Certifications + 0.10×Availability`

See [matching-algorithm.md](docs/matching-algorithm.md) for full details. The matching engine, all 5 individual scorers, and the orchestration service are covered by **46 unit and integration tests**.

## Test Coverage

| Test project | Tests | Scope |
|---|---|---|
| `Matching.Service.Tests` | 46 | Scorers (Skills, Experience, Location, Certifications, Availability), MatchingEngine integration, ScoringMatchingService orchestration |
| `Applications.Api.Tests` | 18 | API integration tests including end-to-end matching (submit → score → verify MatchScore) |
| `Architecture.Tests` | 5 | Project dependency constraints (Shared doesn't reference Infrastructure, etc.) |

## Deployment

### Infrastructure (Azure)
See [infrastructure/terraform](infrastructure/terraform/) for Terraform configs.

### CI/CD
- **CI**: PR checks — lint, build, test, Terraform validate, Docker build
- **CD**: Auto-deploy dev → manual approval staging → canary production

## Production on Azure

### Compute Platform — Azure Container Apps

Azure Container Apps (ACA) was chosen over App Service or AKS for the following reasons:

| Factor | ACA | App Service | AKS |
|---|---|---|---|
| **Scale-to-zero** | ✅ Built-in | ❌ Always on (min 1 instance) | ❌ Needs cluster always running |
| **Dapr integration** | ✅ Native | ❌ Manual setup | ✅ Manual setup |
| **K8s management** | ✅ None (abstracted) | ✅ None | ❌ Full cluster ops |
| **Cost for bursty workloads** | ✅ Low (pay per execution) | ❌ Medium (pay per plan) | ❌ High (pay per node) |
| **Event-driven scaling** | ✅ KEDA-based | ❌ Limited | ✅ Manual KEDA setup |

Recruitment workloads are inherently event-driven — applications spike when a new job is posted and drop off between campaigns. ACA's scale-to-zero and KEDA-based event-driven scaling (queue length, HTTP concurrency) match this perfectly without paying for idle capacity.

### Deployment (Azure)

Each service is deployed as a separate ACA revision with a blue-green strategy:

```
Build → Push to ACR → Deploy to ACA dev (auto) → Smoke test → 
Promote to staging (manual approval) → Canary (10% traffic) → Full prod
```

| Environment | ACA Revision Mode | Ingress | Scaling |
|---|---|---|---|
| **Dev** | Single revision | Internal + APIM | 0-1 replicas |
| **Staging** | Blue-green | Internal + APIM | 1-3 replicas |
| **Production** | Blue-green | APIM only | 1-10 replicas (KEDA) |

### Terraform

The Terraform configs in `infrastructure/terraform/` are illustrative and reflect what a production deployment would look like:

```
infrastructure/terraform/
├── main.tf          # Resource group, ACA environment, ACA apps, SQL Server,
│                    # Service Bus, App Insights, Storage Account, Redis Cache
├── variables.tf     # Environment-aware variables (sku, capacity, tags)
├── outputs.tf       # Connection strings, endpoints, instrumentation key
├── dev.tfvars       # Dev overrides (small SKUs, single replica)
├── staging.tfvars   # Staging overrides
└── prod.tfvars      # Production overrides (HA SKUs, multi-replica)
```

Key resources defined in `main.tf`:
- **Azure Container Apps Environment** with workload profiles
- **ACA Apps** per service (Applications API, Matching Service, Compliance API)
- **Azure SQL Database** with geo-replication (prod)
- **Azure Service Bus** namespace with topics per event type
- **Application Insights** for distributed tracing and log aggregation
- **Azure Storage Account** for document uploads (Blob) and audit logs (Table)
- **Azure Cache for Redis** for session state and match-result caching
- **Managed Identities** assigned to each ACA app for service-to-service auth

### Monitoring

| Layer | Tool | What's Monitored |
|---|---|---|
| **Logs** | Application Insights + Seq | Structured logs, exceptions, dependency calls |
| **Metrics** | App Insights / Container Insights | CPU, memory, request rate, latency (p50/p95/p99) |
| **Alerts** | Azure Monitor | Error rate > 1%, latency > 5s, queue depth > 100 |
| **Dashboards** | Azure Dashboard + Seq | Live site dashboard (requests, errors, matching throughput) |
| **Distributed tracing** | App Insights + Dapr | End-to-end trace from API → event bus → matching → callback |
| **Health probes** | ACA liveness + readiness | `/health` endpoint on each service |

### Quality Gates Before Code Ships

| Stage | Gate | Tool |
|---|---|---|
| **Local** | Build + all 69 tests pass | `dotnet build && dotnet test` |
| **PR** | Code style consistency | `.editorconfig` + `dotnet format --verify-no-changes` |
| **PR** | Architecture constraints | NetArchTest (Shared doesn't depend on Infrastructure, etc.) |
| **PR** | No breaking API changes | OpenAPI diff check (planned) |
| **PR** | Terraform valid | `terraform validate` + `terraform fmt -check` |
| **PR** | Container builds | `docker build` succeeds |
| **Merge to main** | Auto-deploy to dev | CI passes → CD triggers |
| **Pre-prod** | Smoke tests + integration + matching flow tests | `dotnet test` against staging environment |
| **Production** | Canary (10% → 100%) | Monitor error rate + latency before full roll |

## AI Tools Used

- **OpenCode** — Agentic coding assistant
- **Model** — Big Pickle

## Local Infrastructure Credentials

Services started via `docker-compose up -d` use the following default credentials:

| Service | Port | Username | Password | Notes |
|---|---|---|---|---|
| **SQL Server** | `1433` | `sa` | `WorkPac_Dev_2024!` | `TrustServerCertificate=true` |
| **RabbitMQ** (AMQP) | `5900` | `guest` | `guest` | |
| **RabbitMQ** (UI) | `15672` | `guest` | `guest` | Management console at `http://localhost:15672` |
| **Seq** (ingestion) | `5441` | `admin` | `WorkPac_Seq_2024!` | |
| **Seq** (UI) | `8081` | `admin` | `WorkPac_Seq_2024!` | Log viewer at `http://localhost:8081` |
| **Azurite** (blob) | `10000` | `devstoreaccount1` | `Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==` | Default Azurite account |
| **Azurite** (queue) | `10001` | Same | Same | |
| **Azurite** (table) | `10002` | Same | Same | |

## Tech Stack

| Component | Technology |
|---|---|
| Runtime | .NET 8 |
| API Style | Minimal APIs, Vertical Slices |
| Database | SQL Server (EF Core) |
| Message Bus | RabbitMQ (dev) / Azure Service Bus (prod) |
| Storage | Local filesystem (dev) / Azure Blob (prod) |
| Infrastructure | Azure Container Apps, Terraform |
| CI/CD | GitHub Actions |
| Testing | xUnit, FluentAssertions, NSubstitute, NetArchTest |
