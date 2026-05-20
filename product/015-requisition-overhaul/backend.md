# Technical Design - Backend

## Schema Changes

### New Entities

#### Department
- `Guid Id`
- `string Name`
- `bool IsActive`

#### SalaryBand
- `Guid Id`
- `string Name`
- `decimal MinAmount`
- `decimal MaxAmount`
- `string Currency` (default "USD")

#### CostCenter
- `Guid Id`
- `string Code`
- `string Name`

#### RequisitionOpening
- `Guid Id`
- `Guid RequisitionId` (FK)
- `int SequenceNumber`
- `DateTime? TargetStartDate`
- `Guid? CandidateId` (FK to Candidate, null if unfilled)
- `OpeningStatus Status` (Open, Filled, Cancelled)

### Modified Requisition
- `Guid DepartmentId` (FK)
- `Guid SalaryBandId` (FK)
- `Guid CostCenterId` (FK)
- `List<RequisitionOpening> Openings` (Collection)
- Remove: `string Department`, `decimal? SalaryMin`, `decimal? SalaryMax`, `string Currency`, `string CostCenter`.

### Modified Application
- `Guid? RequisitionOpeningId` (FK, nullable)
- Logic: When status set to `Hired` (or equivalent resolution), user must select an available `RequisitionOpening`.

## API Contracts

### Admin Endpoints
- `GET /api/departments`
- `POST /api/departments`
- `GET /api/salary-bands`
- `POST /api/salary-bands`
- `GET /api/cost-centers`
- `POST /api/cost-centers`

### Requisition Updates
- `POST /api/requisitions` - Now requires IDs for Department, SalaryBand, CostCenter.
- `GET /api/requisitions/{id}` - Include Openings list.
