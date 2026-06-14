# Data Model

## Entity Relationship Diagram

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│  JobPosting  │       │  Application │       │   Candidate  │
├──────────────┤       ├──────────────┤       ├──────────────┤
│ Id (PK)      │──┐    │ Id (PK)      │    ┌──│ Id (PK)      │
│ ClientId     │  └───►│ JobPostingId │    │  │ FirstName    │
│ Title        │       │ CandidateId  │◄───┘  │ LastName     │
│ Description  │       │ Status       │       │ Email        │
│ Category     │       │ CoverLetter  │       │ Phone        │
│ Location     │       │ Documents    │       │ Location     │
│ PayRate      │       │ MatchScore   │       │ Skills[]     │
│ RequiredCerts│       │ StatusHistory│       │ Certs[]      │
│ Status       │       │ CreatedAt    │       │ ExperienceYrs│
│ PublishedAt  │       │ UpdatedAt    │       │ Status       │
└──────────────┘       └──────────────┘       └──────────────┘
                              │
                              │ 1
                              ▼
                       ┌──────────────┐
                       │  MatchResult │
                       ├──────────────┤
                       │ Id (PK)      │
                       │ AppId (FK)   │
                       │ CandidateId  │
                       │ JobPostingId │
                       │ Score        │
                       │ Breakdown    │
                       │ CalculatedAt │
                       └──────────────┘
```

## Core Entities

### JobPosting
- **Status workflow**: Draft → Published → Filled/Expired/Cancelled
- **Categories**: Mining, Construction, Industrial, Engineering, Healthcare, Trades
- **Compliance requirements**: site-specific inductions (Standard 11, RIISS), licenses, medicals

### Application
- **Status workflow**: Draft → Submitted → UnderReview → Shortlisted → InterviewScheduled → Interviewed → Offered → Accepted → Mobilising → Placed
- **Alternate paths**: OnHold (from Submitted or UnderReview) resumes back to UnderReview
- **Terminal states**: Rejected and Withdrawn reachable from most non-terminal states
- **Duplicate prevention**: unique index on (JobPostingId, CandidateId)
- **Documents**: resumes, cover letters, cert copies stored in blob storage

### Candidate
- **Skills**: free-text tags for matching via Jaccard similarity
- **Certifications**: validated against job requirements
- **Location preferences**: FIFO/DIDO willing flag impacts matching score
- **Status**: Active, Placed, Unavailable, Blacklisted

### MatchResult
- Pre-computed on application submission (async via event bus)
- **Score range**: 0.0 to 1.0
- **Weighting**: Skills (30%), Experience (25%), Location (15%), Certifications (20%), Availability (10%)

## Value Objects

### DocumentReference
- Embedded value object on Application (`Documents[]`)
- Stored as JSON column in SQL Server
- Fields: `Id` (Guid), `FileName`, `ContentType`, `SizeBytes`, `BlobPath`, `UploadedAt`
- Listed via `GET /v1/documents` (all) or `GET /v1/applications/{id}/documents` (per application)
