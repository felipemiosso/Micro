using Micro.API.Data.Models;
using Micro.API.Endpoints.CustomFields;

namespace Micro.API.Endpoints.Requisition;

public record RequisitionOpeningDto(int SequenceNumber, DateTime? TargetStartDate);

public record CreateRequisitionRequest(
    string Title, 
    Guid DepartmentId, 
    Guid SalaryBandId, 
    Guid CostCenterId,
    int OpeningsCount,
    EmploymentType EmploymentType,
    WorkplaceType WorkplaceType,
    string Location,
    string JobDescription,
    bool IsInternalOnly,
    DateTime? TargetStartDate,
    List<RequisitionOpeningDto>? Openings = null,
    List<CustomFieldValueInput>? CustomFieldValues = null,
    List<Guid>? LinkedCustomFieldIds = null
);

public record UpdateRequisitionRequest(
    string Title, 
    Guid DepartmentId, 
    Guid SalaryBandId, 
    Guid CostCenterId,
    int OpeningsCount,
    EmploymentType EmploymentType,
    WorkplaceType WorkplaceType,
    string Location,
    string JobDescription,
    bool IsInternalOnly,
    DateTime? TargetStartDate,
    List<RequisitionOpeningDto>? Openings = null,
    List<CustomFieldValueInput>? CustomFieldValues = null
);

