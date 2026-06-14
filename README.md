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

```bash
# Start infrastructure (SQL Server, RabbitMQ, Azurite, Seq)
docker-compose up -d

# Run the API
dotnet run --project src/Services/WorkPac.Recruitment.Applications.Api

# Run the Matching Service (in another terminal)
dotnet run --project src/Services/WorkPac.Recruitment.Matching.Service
```

The API is available at `http://localhost:5001/swagger`

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
├── Matching.Service.Tests/    # Unit tests for matching engine
├── Applications.Api.Tests/    # Integration tests
└── Architecture.Tests/        # Architectural constraint tests
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
| GET | `/v1/jobs/{jobId}/applications` | List applications for job |
| GET | `/v1/applications/{id}` | Get application detail |
| PATCH | `/v1/applications/{id}/status` | Update application status |
| POST | `/v1/applications/{id}/documents` | Upload document |
| DELETE | `/v1/applications/{id}` | Withdraw application |
| GET | `/v1/candidates/{id}/applications` | My applications |
| GET | `/health` | Health check |

## Candidate Matching Score

`Score = 0.30×Skills + 0.25×Experience + 0.15×Location + 0.20×Certifications + 0.10×Availability`

See [matching-algorithm.md](docs/matching-algorithm.md) for full details.

## Deployment

### Infrastructure (Azure)
See [infrastructure/terraform](infrastructure/terraform/) for Terraform configs.

### CI/CD
- **CI**: PR checks — lint, build, test, Terraform validate, Docker build
- **CD**: Auto-deploy dev → manual approval staging → canary production

## AI Tools Used

- **GitHub Copilot** — API endpoint scaffolding, test writing
- **AI assistant** — Architecture exploration, code generation, plan refinement, Azure services research, domain context from WorkPac's public materials
- **PlantUML** — Architecture and sequence diagrams

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
