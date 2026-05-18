# Candidate Profile Separation — Backend Specification

### Schema Changes

#### `Candidate` Entity (New)
```csharp
public class Candidate
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; // Unique Index
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
```

#### `Application` Entity (Modified)
- Remove: `CandidateName`, `CandidateEmail`, `CandidatePhone`.
- Add: `CandidateId` (Guid, Foreign Key).
- Keep: `Status`, `ArchivalResolution`, `AppliedAt`, `JobPostingId`.

### Migration Strategy
1. Create `Candidates` table.
2. Extract unique `CandidateEmail` from `Applications`.
3. Insert into `Candidates` (pick first `CandidateName` and `CandidatePhone` found).
4. Update `Applications` set `CandidateId` where email match.
5. Drop old columns from `Applications`.

### API Updates

- **Public Apply**: Check if email exist in `Candidates`. If no, create. Link application to `CandidateId`.
- **Admin Application Detail**: Include `Candidate` object in response.
- **Admin Candidate Update**: `PUT /api/admin/candidates/{id}` to update profile info.
