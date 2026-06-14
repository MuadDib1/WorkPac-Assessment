# Architecture

## System Context

WorkPac is one of Australia's largest workforce solutions providers, placing over 1.5M candidates across mining, construction, industrial, engineering, and healthcare sectors.

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                    API Gateway (Azure APIM)                  │
│        Rate limiting, auth (Azure AD B2C), routing,          │
│        API versioning                                        │
└──────────┬───────────────────────┬────────────────┬──────────┘
           │                       │                │
     ┌─────▼──────────┐     ┌──────▼──────┐  ┌──────▼──────┐
     │ Applications   │     │  Matching   │  │ Compliance  │
     │     API        │     │  Service    │  │    API      │
     │ (Jobs, Apps,   │     │             │  │             │
     │  Candidates,   │     │             │  │             │
     │  Documents)    │     │             │  │             │
     └──┬──────────┬──┘     └──────┬──────┘  └──────┬──────┘
        │          │               │                │
        │          └───────────────┼────────────────┘
        │                          │
        ▼                          ▼
  ┌──────────┐            ┌──────────────────────────────┐
  │  Event   │            │         Azure SQL            │
  │   Bus    │            │  (Jobs, Applications,        │
  │(RabbitMQ │            │   Candidates, Matches)       │
  │  /SB)    │            │                              │
  └──────────┘            └──────────────────────────────┘
```

## Service Responsibilities

| Service | Description | Tech |
|---|---|---|
| **Applications.Api** | Jobs, applications, candidates, documents, status workflow | ASP.NET Core Minimal API |
| **Matching.Service** | Background worker scoring candidates against jobs | .NET Worker Service |
| **Compliance.Api** | Verify certs, licenses, medicals, site inductions | ASP.NET Core Minimal API |

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

## Authentication & Authorization

Authentication uses **Azure AD B2C** with the **API Management (APIM)** gateway as the token validation boundary:

```
Candidate / Recruiter → Azure AD B2C → JWT → APIM (validate) → Backend Service ([Authorize])
```

| Pattern | Detail |
|---|---|
| **Identity Provider** | Azure AD B2C with custom policies. Recruiters authenticate via corporate Azure AD; candidates use social IdPs (Microsoft, Google, LinkedIn) |
| **Token validation** | APIM validates JWT signatures, expiry, issuer, and audience before requests reach backend services |
| **Backend authorization** | ASP.NET Core `[Authorize]` attributes with role policies (`Recruiter`, `Candidate`, `Admin`) |
| **Service-to-service** | Managed identities (MSI) with Azure AD tokens for inter-service calls (e.g., Applications API → Matching Service) |
| **Local dev** | `anonymousAuthentication: true` in launchSettings; auth can be enabled by configuring OpenID Connect against a test B2C tenant or local IdentityServer |

### Role-Based Access Control

| Endpoint Group | Recruiter | Candidate | Admin |
|---|---|---|---|
| List jobs, candidates, documents | ✓ | ✓ | ✓ |
| Submit application | ✓ | ✓ (own) | ✓ |
| View applications | ✓ | ✓ (own) | ✓ |
| Update status, withdraw | ✓ | ✓ (own) | ✓ |
| Upload documents | ✓ | ✓ (own) | ✓ |
| Compliance checks | ✓ | ✗ | ✓ |
| Matching results | ✓ | ✓ (own) | ✓ |

## API Versioning

The API uses **URL path versioning** (`/v1/`, `/v2/`). Version `v1` is current.

### Multi-layered Versioning Strategy

| Layer | Mechanism |
|---|---|
| **URL path** | `/v{major}/` prefix on all route templates (primary) |
| **Content negotiation** | `Accept: application/vnd.workpac.v{major}+json` (planned for APIM) |
| **Query string** | `?api-version=X.X` fallback for backwards compatibility |
| **Header** | `X-Api-Version` for internal routing (planned) |

### Compatibility Rules

- **Additive changes** (new fields, new endpoints) — no version bump needed
- **Breaking changes** (remove field, change type, rename endpoint) — new major version
- **Deprecation** — deprecated versions return `Sunset` and `Deprecation` HTTP headers; maintained for at least 6 months
- **Swagger** — each version gets its own SwaggerDoc; developer portal lists all active versions
