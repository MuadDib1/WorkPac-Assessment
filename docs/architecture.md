# Architecture

## System Context

WorkPac is one of Australia's largest workforce solutions providers, placing over 1.5M candidates across mining, construction, industrial, engineering, and healthcare sectors.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                            API Gateway (Azure APIM)                      │
│       Rate limiting, auth (Azure AD B2C), routing, API versioning        │
└──────────┬──────────┬──────────┬──────────┬──────────┬──────────────────┘
           │          │          │          │          │
     ┌─────▼──┐  ┌───▼───┐  ┌──▼────┐  ┌──▼───┐  ┌──▼────┐
     │ Jobs   │  │ Apps  │  │Cand.  │  │Match │  │Compl. │
     │Service │  │Service│  │Service│  │Service│  │Service│
     └───┬────┘  └───┬───┘  └───┬───┘  └───┬───┘  └───┬───┘
         │           │          │           │           │
    ┌────┴───────────┴──────────┴───────────┴───────────┴────┐
    │                 Event Bus (RabbitMQ / Service Bus)       │
    │  Topics: job.created, application.submitted,             │
    │  match.completed, compliance.verified                    │
    └────────────────────────┬────────────────────────────────┘
                             │
    ┌────────────────────────┴────────────────────────────────┐
    │                     Data Layer                            │
    │  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │
    │  │ Azure SQL    │  │ Azure Cosmos │  │ Azure Search  │  │
    │  │ (Jobs, Apps, │  │ DB (Profiles,│  │ (Full-text    │  │
    │  │ Candidates)  │  │ Audit Logs)  │  │ + AI matching)│  │
    │  └──────────────┘  └──────────────┘  └───────────────┘  │
    └──────────────────────────────────────────────────────────┘
```

## Service Responsibilities

| Service | Description | Tech |
|---|---|---|
| **Jobs.Api** | CRUD for job postings, publishing workflow | ASP.NET Core Minimal API |
| **Applications.Api** | Submit, withdraw, track applications; status workflow (PRIMARY) | ASP.NET Core Minimal API |
| **Candidates.Api** | Candidate profiles, skills, certs, work history | ASP.NET Core Minimal API |
| **Matching.Service** | Background worker scoring candidates against jobs | .NET Worker Service |
| **Compliance.Api** | Verify certs, licenses, medicals, site inductions | ASP.NET Core Minimal API |
| **Notifications.Service** | Email/SMS dispatch (future) | .NET Worker Service |

## Key Decisions

### Why Azure Container Apps?
- Serverless scale-to-zero for non-peak hours (recruitment is event-driven)
- Built-in Dapr support for pub/sub, state management, and service discovery
- No Kubernetes management overhead
- Cheaper than AKS for this workload profile

### Why Event-Driven?
- Application submission triggers matching asynchronously
- Status changes trigger notifications
- Decouples services — Matching can scale independently
- Outbox pattern ensures reliable delivery

### Why CQRS on Matching?
- Reads: pre-computed match scores, searchable via Azure Search
- Writes: transactional storage of applications and profiles
- Matching can be re-run on profile changes without blocking writes
