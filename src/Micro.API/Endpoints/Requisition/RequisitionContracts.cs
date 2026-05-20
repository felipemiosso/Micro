using Micro.API.Data.Models;

namespace Micro.API.Endpoints.Requisition;

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
    DateTime? TargetStartDate
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
    DateTime? TargetStartDate
);
