# Matching Algorithm

## Formula

```
MatchScore = 0.30 × SkillsMatch
           + 0.25 × ExperienceMatch
           + 0.15 × LocationMatch
           + 0.20 × CertificationsMatch
           + 0.10 × AvailabilityMatch

Range: 0.0 (no match) to 1.0 (perfect match)
```

## Component Scoring

### SkillsMatch (Jaccard Similarity)
```
Score = |Skills_Candidate ∩ Skills_Required| / |Skills_Candidate ∪ Skills_Required|
```

- Case-insensitive comparison
- Returns 1.0 if no skills are required
- Returns 0.0 if candidate has no skills

### ExperienceMatch
```
Score = min(Experience_Candidate / Experience_Required, 1.0)
```

- Returns 1.0 if no minimum experience required
- Caps at 1.0 (extra experience doesn't over-score)

### LocationMatch
| Scenario | Score |
|---|---|
| Same site (e.g., both "Peak Downs") | 1.0 |
| Different site but FIFO willing | 0.8 |
| Same state/region | 0.5 |
| No location overlap | 0.0 |

### CertificationsMatch
```
Score = Required_Certs_Held_and_Valid / Total_Required_Certs
```

- Returns 1.0 if no certs required
- Only counts valid (non-expired) certifications

### AvailabilityMatch
| Candidate Status | Score |
|---|---|
| Active | 1.0 |
| Placed | 0.0 |
| Unavailable | 0.0 |

## Weight Rationale

| Factor | Weight | Reason |
|---|---|---|
| Skills | 30% | Strongest predictor of job fit for blue-collar roles |
| Experience | 25% | Years in role matters for safety-critical mining work |
| Location | 15% | FIFO willingness and site access are key constraints |
| Certifications | 20% | Site access is impossible without correct compliance docs |
| Availability | 10% | Immediate availability is nice-to-have but not decisive |

## Scaling Strategy

### Phase 1 (Current)
- Synchronous scoring via MatchingEngine
- Triggered on application submission via event bus
- Results cached in database

### Phase 2 (Future)
- Pre-computed candidate skill vectors (updated on profile change)
- Redis cache for match results with TTL
- Batch matching for bulk hiring (mine shutdowns: 50+ roles simultaneously)

### Phase 3 (ML-Augmented)
- Train a regression model on historical placement data
- Features: skills, experience, location, certs, job category, client
- A/B test ML vs. weighted formula
- Auto-adjust weights based on placement outcome data
